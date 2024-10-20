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

     
        
        public void DisplayHashedPassword()
        {
            string plainPassword = "Password123!";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            Console.WriteLine($"Plain Password: {plainPassword}");
            Console.WriteLine($"Hashed Password: {hashedPassword}");
        }


        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.Email) || string.IsNullOrEmpty(vm.Password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View(vm);
            }
            DisplayHashedPassword();
            string role = GetRole(vm.Email, vm.Password);

            if (!string.IsNullOrEmpty(role))
            {
                switch (role.ToLower())
                {
                    case "admin":
                        await SignInAdmin(vm.Email, role);
                        return RedirectToAction("AdminDashboard", "Admin");
                    case "surgeon":
                        await SignInSurgeon(vm.Email, role);
                        return RedirectToAction("Dashboard", "Surgeon");
                    case "nurse":
                        await SignInNurse(vm.Email, role);
                        return RedirectToAction("Dashboard", "Nurse");
                    case "pharmacist":
                        await SignInPharmacist(vm.Email, role);
                        return RedirectToAction("PharmacistDashboard", "Pharmacist");
                    case "anaesthesiologist":
                        await SignInAnaesthesiologist(vm.Email, role);
                        return RedirectToAction("AnaesthesiologistDashboard", "Anaesthesiologist");
                    default:
                        ModelState.AddModelError("", "Invalid role.");
                        break;
                }
            }
            else
            {
                ModelState.AddModelError("", "Incorrect email or password.");
            }

            return View(vm);
        }

        private async Task SignInAdmin(string email, string role)
        {
            var adminDetails = _helper.GetAdminByEmail("SELECT * FROM Admin WHERE Email=@Email", email);
            if (adminDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, adminDetails.AdminID.ToString()),
                    new Claim("Role", role),
                    new Claim("AdminID", adminDetails.AdminID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("Username", adminDetails.Username);
            }
        }

        private async Task SignInSurgeon(string email, string role)
        {
            var surgeonDetails = _helper.GetSurgeonByEmail(@"
                SELECT s.SurgeonID, s.UserID, u.Username ,u.Name, u.Surname
                FROM Surgeon s
                INNER JOIN [User] u ON s.UserID = u.UserID
                WHERE u.Email = @Email", email);
            if (surgeonDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, surgeonDetails.SurgeonID.ToString()),
                    new Claim(ClaimTypes.Name, $" {surgeonDetails.Name} {surgeonDetails.Surname}"), // Add this line
                    new Claim("Role", role),
                    new Claim("SurgeonID", surgeonDetails.SurgeonID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Username", surgeonDetails.Username); // Updated to get username
                HttpContext.Session.SetString("Name", surgeonDetails.Name);
                HttpContext.Session.SetString("Surname", surgeonDetails.Surname);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInNurse(string email, string role)
        {
            var nurseDetails = _helper.GetNurseByEmail(@"
                SELECT n.NurseID, n.UserID, u.Username, u.Name, u.Surname 
                FROM Nurse n
                INNER JOIN [User] u ON n.UserID = u.UserID
                WHERE u.Email = @Email", email);
            if (nurseDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, nurseDetails.NurseID.ToString()),
                     new Claim(ClaimTypes.Name, $" {nurseDetails.Name} {nurseDetails.Surname}"),
                    new Claim("Role", role),
                    new Claim("NurseID", nurseDetails.NurseID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Username", nurseDetails.Username);
                HttpContext.Session.SetString("Name", nurseDetails.Name);
                HttpContext.Session.SetString("Surname", nurseDetails.Surname);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("NurseID", nurseDetails.NurseID.ToString());
            }
        }

        private async Task SignInPharmacist(string email, string role)
        {
            var pharmacistDetails = _helper.GetPharmacistByEmail(@"
                SELECT p.PharmacistID, p.UserID, u.Username 
                FROM Pharmacist p
                INNER JOIN [User] u ON p.UserID = u.UserID
                WHERE u.Email = @Email", email);
            if (pharmacistDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, pharmacistDetails.PharmacistID.ToString()),
                    new Claim("Role", role),
                    new Claim("PharmacistID", pharmacistDetails.PharmacistID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);


                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Username", pharmacistDetails.Username);
                HttpContext.Session.SetString("Role", role);
            }
        }

        private async Task SignInAnaesthesiologist(string email, string role)
        {
            var anaesthesiologistDetails = _helper.GetAnaesthesiologistByEmail(@"
                SELECT a.AnaesthesiologistID, a.UserID, u.Username ,u.Name,u.Surname
                FROM Anaesthesiologist a
                INNER JOIN [User] u ON a.UserID = u.UserID
                WHERE u.Email = @Email", email);
            if (anaesthesiologistDetails != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, anaesthesiologistDetails.AnaesthesiologistID.ToString()),
                     new Claim(ClaimTypes.Name, $" {anaesthesiologistDetails.Name} {anaesthesiologistDetails.Surname}"), // Add this line
                    new Claim("Role", role),
                    new Claim("AnaesthesiologistID", anaesthesiologistDetails.AnaesthesiologistID.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("Username", anaesthesiologistDetails.Username);
                HttpContext.Session.SetString("Name", anaesthesiologistDetails.Name);
                HttpContext.Session.SetString("Surname", anaesthesiologistDetails.Surname);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString("AnaesthesiologistID", anaesthesiologistDetails.AnaesthesiologistID.ToString());
            }
        }

        private string GetRole(string email, string password)
        {
            try
            {
                // Check in Admin table first
                string adminQuery = "SELECT * FROM Admin WHERE Email=@Email";
                var adminDetails = _helper.GetAdminByEmail(adminQuery, email);

                if (adminDetails != null && BCrypt.Net.BCrypt.Verify(password, adminDetails.HashedPassword))
                {
                    var roles = _helper.GetEntityById("SELECT * FROM [Role] WHERE RoleId=@RoleId", adminDetails.RoleId.ToString());
                    return roles.Name;
                }

                // Check in User table if not admin
                string userQuery = "SELECT * FROM [User] WHERE Email=@Email";
                var userDetails = _helper.GetUserByEmail(userQuery, email);

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
