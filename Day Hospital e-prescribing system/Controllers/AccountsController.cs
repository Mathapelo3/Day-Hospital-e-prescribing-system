using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.Utility;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CommonHelper _helper;

        public AccountsController(IConfiguration config)
        {
            _config = config;
            _helper = new CommonHelper(_config);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            ViewBag.Roles = GetRoles();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            ViewBag.Roles = GetRoles();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            bool userExists = _helper.UserAlreadyExists(vm.Username);

            if (userExists)
            {
                TempData["ErrorMessage"] = "Username already exists!";
                return View(vm);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            int adminID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "AdminID").Value);

            string query = "INSERT INTO [User] (Name, Surname, Email, ContactNo, HCRNo, Username, HashedPassword, AdminID, RoleId) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@Name, @Surname, @Email, @ContactNo, @HCRNo, @Username, @HashedPassword, @AdminID, @RoleId)";

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", vm.Name);
                        command.Parameters.AddWithValue("@Surname", vm.Surname);
                        command.Parameters.AddWithValue("@Email", vm.Email);
                        command.Parameters.AddWithValue("@ContactNo", vm.ContactNo);
                        command.Parameters.AddWithValue("@HCRNo", vm.HCRNo);
                        command.Parameters.AddWithValue("@Username", vm.Username);
                        command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                        command.Parameters.AddWithValue("@AdminID", adminID);
                        command.Parameters.AddWithValue("@RoleId", vm.RoleId);

                        int userId = (int)await command.ExecuteScalarAsync();

                        await InsertRoleSpecificUser(vm.RoleId, userId);

                        TempData["SuccessMessage"] = "User successfully added into the system.";
                        return RedirectToAction("MedicalProfessionals");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                // Log the exception
                return View(vm);
            }
        }

        private async Task InsertRoleSpecificUser(int roleId, int userId)
        {
            string roleTable = roleId switch
            {
                2 => "Pharmacist",
                3 => "Surgeon",
                4 => "Nurse",
                5 => "Anaesthesiologist",
                _ => throw new ArgumentException("Invalid role ID")
            };

            string query = $"INSERT INTO [{roleTable}] (UserID) VALUES (@UserID)";
            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert into {roleTable} table: {ex.Message}");
            }
        }

        public List<SelectListItem> GetRoles()
        {
            List<SelectListItem> roles = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT RoleId, Name FROM [Role]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new SelectListItem
                                {
                                    Value = reader["RoleId"].ToString(),
                                    Text = reader["Name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Failed to fetch roles: {ex.Message}");
            }

            return roles;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.Username) || string.IsNullOrEmpty(vm.Password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View(vm);
            }

            string role = GetRole(vm.Username, vm.Password);

            if (!string.IsNullOrEmpty(role))
            {
                switch (role.ToLower())
                {
                    case "admin":
                        await SignInAdmin(vm.Username, role);
                        return RedirectToAction("AdminDashboard", "Admin");
                    case "surgeon":
                        await SignInSurgeon(vm.Username, role);
                        return RedirectToAction("Dashboard", "Surgeon");
                    case "nurse":
                        await SignInNurse(vm.Username, role);
                        return RedirectToAction("Dashboard", "Nurse");
                    case "pharmacist":
                        await SignInPharmacist(vm.Username, role);
                        return RedirectToAction("PharmacistDashboard", "Pharmacist");
                    case "anaesthesiologist":
                        await SignInAnaesthesiologist(vm.Username, role);
                        return RedirectToAction("AnaesthesiologistDashboard", "Anaesthesiologist");
                    default:
                        ModelState.AddModelError("", "Invalid role.");
                        break;
                }
            }
            else
            {
                ModelState.AddModelError("", "Incorrect username or password.");
            }

            return View(vm);
        }

        private async Task SignInAdmin(string username, string role)
        {
            var adminDetails = _helper.GetAdminByUsername("SELECT * FROM Admin WHERE Username=@Username", username);
            if (adminDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, adminDetails.AdminID.ToString()),
                    new Claim("Role", role),
                    new Claim("AdminID", adminDetails.AdminID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInSurgeon(string username, string role)
        {
            var surgeonDetails = _helper.GetSurgeonByUsername(@"
                SELECT s.SurgeonID, s.UserID, u.HashedPassword 
                FROM Surgeon s
                INNER JOIN [User] u ON s.UserID = u.UserID
                WHERE u.Username = @Username", username);
            if (surgeonDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, surgeonDetails.SurgeonID.ToString()),
                    new Claim("Role", role),
                    new Claim("SurgeonID", surgeonDetails.SurgeonID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInNurse(string username, string role)
        {
            var nurseDetails = _helper.GetNurseByUsername(@"
                SELECT n.NurseID, n.UserID, u.HashedPassword 
                FROM Nurse n
                INNER JOIN [User] u ON n.UserID = u.UserID
                WHERE u.Username = @Username", username);
            if (nurseDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, nurseDetails.NurseID.ToString()),
                    new Claim("Role", role),
                    new Claim("NurseID", nurseDetails.NurseID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInPharmacist(string username, string role)
        {
            var pharmacistDetails = _helper.GetPharmacistByUsername(@"
                SELECT p.PharmacistID, p.UserID, u.HashedPassword 
                FROM Pharmacist p
                INNER JOIN [User] u ON p.UserID = u.UserID
                WHERE u.Username = @Username", username);
            if (pharmacistDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, pharmacistDetails.PharmacistID.ToString()),
                    new Claim("Role", role),
                    new Claim("PharmacistID", pharmacistDetails.PharmacistID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInAnaesthesiologist(string username, string role)
        {
            var anaesthesiologistDetails = _helper.GetAnaesthesiologistByUsername(@"
                SELECT a.AnaesthesiologistID, a.UserID, u.HashedPassword 
                FROM Anaesthesiologist a
                INNER JOIN [User] u ON a.UserID = u.UserID
                WHERE u.Username = @Username", username);
            if (anaesthesiologistDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, anaesthesiologistDetails.AnaesthesiologistID.ToString()),
                    new Claim("Role", role),
                    new Claim("AnaesthesiologistID", anaesthesiologistDetails.AnaesthesiologistID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private string GetRole(string username, string password)
        {
            try
            {
                // Check in Admin table first
                string adminQuery = "SELECT * FROM Admin WHERE Username=@Username";
                var adminDetails = _helper.GetAdminByUsername(adminQuery, username);

                if (adminDetails != null && BCrypt.Net.BCrypt.Verify(password, adminDetails.HashedPassword))
                {
                    var roles = _helper.GetEntityById("SELECT * FROM [Role] WHERE RoleId=@RoleId", adminDetails.RoleId.ToString());
                    return roles.Name;
                }

                // Check in User table if not admin
                string userQuery = "SELECT * FROM [User] WHERE Username=@Username";
                var userDetails = _helper.GetUserByUsername(userQuery, username);

                if (userDetails != null && BCrypt.Net.BCrypt.Verify(password, userDetails.HashedPassword))
                {
                    var roles = _helper.GetEntityById("SELECT * FROM [Role] WHERE RoleId=@RoleId", userDetails.RoleId.ToString());
                    return roles.Name;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Error in GetRole: {ex.Message}");
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Accounts");
        }
    }
}
