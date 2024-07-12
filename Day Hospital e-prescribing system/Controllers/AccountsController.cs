using Day_Hospital_e_prescribing_system.Helper;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.Utility;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;


namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AccountsController : Controller
    {
        private IConfiguration _config;
        CommonHelper _helper;
       

        public AccountsController(IConfiguration config)
        {
            _config = config;
            _helper = new CommonHelper(_config);
            
        }

        public IActionResult Index()
        {
            return View();
        }

        /*Register into the system (admin)*/

        public IActionResult Register()
        {
            //List<Role> roles = new List<Role>();
            //roles =  (from x in _config.Roles select x).ToList();
            //roles.Insert(0, new Role { RoleId = 0, Name = "Select Role" });

            //ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel vm)
        {
            bool userExists = false;
            try
            {
                // Database operation to check if user already exists
                userExists = _helper.UserAlreadyExists(vm.Username);
            }
            catch (Exception ex)
            {
                // Handle exceptions that occur during the database operation
                TempData["ErrorMessage"] = "An error occurred during registration: " + ex.Message;
                return View("Register");
            }

            //bool userExists = _helper.UserAlreadyExists(vm.Username);
            if (userExists)
            {
                TempData["ErrorMessage"] = "Username Already Exists!";
                return View("Register");
            }

            // Assuming you have a method to hash the password
            string hashedPassword = HashPassword(vm.Password);

            // Ensure a role is selected or assigned
            int roleId = vm.RoleId != 0 ? vm.RoleId : 6; // This is a correct statement


            string query = "INSERT INTO [User](Name,Email,ContactNo,HCRNo,Username, Password,AdminID,SpecializationID, RoleId) VALUES (@Name,@Email,@ContactNo,@HCRNo,@Username, @Password,@AdminID,@SpecializationID, @RoleId)";
            using (SqlConnection connection = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", vm.Name);
                    command.Parameters.AddWithValue("@Email", vm.Email);
                    command.Parameters.AddWithValue("@ContactNo", vm.ContactNo);
                    command.Parameters.AddWithValue("@HCRNo", vm.HCRNo);
                    command.Parameters.AddWithValue("@Username", vm.Username);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@AdminID", vm.AdminID);
                    
                    command.Parameters.AddWithValue("@RoleId", roleId);
                    
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        EntryIntoSession(vm.Username);
                        TempData["SuccessMessage"] = "User successfully added into the system";
                        return View();
                    }
                }
            }

            return View();
        }

        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }



        private void EntryIntoSession(string Username) 
        {
            HttpContext.Session.SetString("Username", Username);
        }

        /*Login into the system*/

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.Username) || string.IsNullOrEmpty(vm.Password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View(vm);
            }

            bool isAuthenticated = SignInMethod(vm.Username, vm.Password);
            if (isAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Incorrect username or password.");
            return View("Login", vm);
        }


        private bool SignInMethod(string username, string password)
        {
            bool isAuthenticated = false;
            string userQuery = "SELECT * FROM [User] WHERE Username=@Username";
            var userDetails = _helper.GetUserByUsername(userQuery, username);

            if (userDetails != null && userDetails.Username != null)
            {
                string hashedPassword = PasswordHasher.HashPassword(password);
                if (userDetails.Password == hashedPassword)
                {
                    isAuthenticated = true;
                    string roleQuery = "SELECT * FROM [Role] WHERE RoleId=@RoleId";
                    var roles = _helper.GetEntityById(roleQuery, userDetails.RoleId.ToString());
                    RoleViewModel vm = new RoleViewModel();
                    vm.RoleId = roles.Id;
                    vm.Name = roles.Name;

                    if (vm.Name == "Admin")
                    {
                        HttpContext.Session.SetString("Role", "Admin");
                    }
                    else
                    {
                        HttpContext.Session.SetString("Role", "user");
                    }
                    HttpContext.Session.SetString("Username", userDetails.Username);
                }

                    
            }
            else
            {
                TempData["ErrorMessage"] = "Incorrect Username and Password";
            }
            return isAuthenticated;
        }

    }
}
