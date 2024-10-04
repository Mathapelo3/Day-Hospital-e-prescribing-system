using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.CodeAnalysis.Scripting;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Day_Hospital_e_prescribing_system.Helper;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly CommonHelper _helper;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _config;
        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _helper = new CommonHelper(_config);
        }
        public IActionResult Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public IActionResult AdminDashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        public async Task<IActionResult> DayHospitalRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Fetching all hospital records.");
            var hospitalRecords = await _context.HospitalRecords
            .Include(hr => hr.Suburb)
                .ThenInclude(s => s.City)
                .ThenInclude(c => c.Province)
            .ToListAsync();

            var viewModelList = hospitalRecords.Select(hr => new HospitalRecordViewModel
            {
                HospitalRecordID = hr.HospitalRecordID,
                Name = hr.Name,
                AddressLine1 = hr.AddressLine1,
                AddressLine2 = hr.AddressLine2,
                ContactNo = hr.ContactNo,
                Email = hr.Email,
                PM = hr.PM,
                PMEmail = hr.PMEmail,
                SuburbName = hr.Suburb.Name,
                PostalCode = hr.Suburb.PostalCode,
                CityName = hr.Suburb.City.Name,
                ProvinceName = hr.Suburb.City.Province.Name
            }).ToList();

            return View(viewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> EditHospitalRecord(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Editing hospital record with ID {id}", id);

            var hospitalRecord = await _context.HospitalRecords
                .Include(hr => hr.Suburb)
                .ThenInclude(s => s.City)
                .ThenInclude(c => c.Province)
                .FirstOrDefaultAsync(hr => hr.HospitalRecordID == id);

            if (hospitalRecord == null)
            {
                return NotFound();
            }

            var viewModel = new EditHospitalRecordViewModel
            {
                HospitalRecordID = hospitalRecord.HospitalRecordID,
                Name = hospitalRecord.Name,
                AddressLine1 = hospitalRecord.AddressLine1,
                //AddressLine2 = hospitalRecord.AddressLine2,
                ContactNo = hospitalRecord.ContactNo,
                Email = hospitalRecord.Email,
                PM = hospitalRecord.PM,
                PMEmail = hospitalRecord.PMEmail,
                SuburbID = hospitalRecord.SuburbID,
                SuburbName = hospitalRecord.Suburb.Name,
                PostalCode = hospitalRecord.Suburb.PostalCode,
                CityName = hospitalRecord.Suburb.City.Name,
                ProvinceName = hospitalRecord.Suburb.City.Province.Name,
                Suburbs = await _context.Suburbs.Select(s => new SelectListItem { Value = s.SuburbID.ToString(), Text = s.Name }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalRecord(EditHospitalRecordViewModel viewModel)
        {

            try
            {
                _logger.LogInformation("Starting to update hospital record with ID {id}", viewModel.HospitalRecordID);

                if (!ModelState.IsValid)
                {

                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        foreach (var error in state.Errors)
                        {
                            _logger.LogInformation("Key: {Key}, Error: {ErrorMessage}", key, error.ErrorMessage);
                        }
                    }
                    viewModel.Suburbs = await _context.Suburbs.Select(s => new SelectListItem { Value = s.SuburbID.ToString(), Text = s.Name }).ToListAsync();
                    return View(viewModel);
                }


                var hospitalRecord = await _context.HospitalRecords
                    .Include(hr => hr.Suburb)
                    .ThenInclude(s => s.City)
                    .ThenInclude(c => c.Province)
                    .FirstOrDefaultAsync(hr => hr.HospitalRecordID == viewModel.HospitalRecordID);

                if (hospitalRecord == null)
                {
                    _logger.LogInformation("Hospital record with ID {id} not found", viewModel.HospitalRecordID);
                    return NotFound();
                }

                hospitalRecord.Name = viewModel.Name;
                hospitalRecord.AddressLine1 = viewModel.AddressLine1;
                //hospitalRecord.AddressLine2 = viewModel.AddressLine2;
                hospitalRecord.ContactNo = viewModel.ContactNo;
                hospitalRecord.Email = viewModel.Email;
                hospitalRecord.PM = viewModel.PM;
                hospitalRecord.PMEmail = viewModel.PMEmail;
                hospitalRecord.SuburbID = viewModel.SuburbID;

                _context.Entry(hospitalRecord).State = EntityState.Modified;
                _logger.LogInformation("Updated hospital record with ID {id} in context", viewModel.HospitalRecordID);

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved changes to database for hospital record with ID {id}", viewModel.HospitalRecordID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes to database for hospital record with ID {id}", viewModel.HospitalRecordID);
                    return View(viewModel);
                }

                _logger.LogInformation("Hospital record with ID {id} has been updated", viewModel.HospitalRecordID);

                TempData["SuccessMessage"] = "Hospital record updated successfully.";
                _logger.LogInformation("Redirecting to DayHospitalRecords action");
                return RedirectToAction(nameof(DayHospitalRecords)); // Redirect to DayHospitalRecords action
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hospital record with ID {id}", viewModel.HospitalRecordID);
                return View(viewModel);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSuburbDetails(int suburbID)
        {
            var suburb = await _context.Suburbs
                .Include(s => s.City)
                .ThenInclude(c => c.Province)
                .FirstOrDefaultAsync(s => s.SuburbID == suburbID);

            if (suburb == null)
            {
                return NotFound();
            }

            return Json(new
            {
                postalCode = suburb.PostalCode,
                cityName = suburb.City.Name,
                provinceName = suburb.City.Province.Name
            });
        }

        [HttpGet]
        public IActionResult AddMedicalProfessional()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to the login page or display an error message
                return RedirectToAction("Login", "Account");
            }
            var viewModel = new RegisterViewModel();
            viewModel.Roles = GetRoles().ToList(); // Assign the result to the Roles property
            return View(viewModel);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicalProfessional([Bind("Name,Surname,Email,ContactNo,HCRNo,Username,HashedPassword,RoleId")] RegisterViewModel model)
        {

            model.Roles = GetRoles(); // Ensure Roles is repopulated

            _logger.LogInformation("RoleId selected: {RoleId}", model.RoleId);

            if (model.RoleId <= 0)
            {
                ModelState.AddModelError("RoleId", "Please select a valid role.");
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("ModelState is valid. Processing registration.");

                // Retrieve the AdminID of the logged-in user
                var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
                _logger.LogInformation("Logged-in user's email: {Email}", loggedInUserEmail);

                if (string.IsNullOrEmpty(loggedInUserEmail))
                {
                    _logger.LogError("Logged-in user's email is null or empty.");
                    // Handle the error or return an error message
                }
                var query = "SELECT AdminID, Username, Email, HashedPassword, RoleId FROM Admin WHERE Email = @Email";
                var loggedInUser = _helper.GetAdminByEmail(query, loggedInUserEmail);

                if (loggedInUser != null)
                {
                    int adminId = loggedInUser.AdminID; // Assign value to adminId

                    // Hash the password
                    var hashedPassword = HashPassword(model.HashedPassword);
                    var user = new User
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        ContactNo = model.ContactNo,
                        HCRNo = model.HCRNo,
                        Username = model.Username,
                        HashedPassword = hashedPassword, // Store the hashed password
                        AdminID = adminId, // Set the AdminID to the logged-in user's AdminID
                        RoleId = model.RoleId
                    };

                    _context.Users.Add(user);
                    _logger.LogInformation("User added to context.");

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("User saved to database.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving user to database.");
                        ModelState.AddModelError(string.Empty, "An error occurred while saving the user.");
                        model.Roles = GetRoles().ToList();
                        return View(model);
                    }

                    // Depending on role, add to respective table
                    if (model.RoleId == 3)
                    {
                        var surgeon = new Surgeon { UserID = user.UserID };
                        _context.Surgeons.Add(surgeon);
                        _logger.LogInformation("Surgeon added.");
                    }
                    else if (model.RoleId == 4)
                    {
                        var nurse = new Nurse { UserID = user.UserID };
                        _context.Nurses.Add(nurse);
                        _logger.LogInformation("Nurse added.");
                    }
                    else if (model.RoleId == 2)
                    {
                        var pharmacist = new Pharmacist { UserID = user.UserID };
                        _context.Pharmacist.Add(pharmacist);
                        _logger.LogInformation("Pharmacist added.");
                    }
                    else if (model.RoleId == 5)
                    {
                        var anaesthesiologist = new Anaesthesiologist { UserID = user.UserID };
                        _context.Anaesthesiologists.Add(anaesthesiologist);
                        _logger.LogInformation("Anaesthesiologist added.");
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Role-specific entity saved to database.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving role-specific entity to database.");
                        ModelState.AddModelError(string.Empty, "An error occurred while saving the role-specific entity.");
                        model.Roles = GetRoles().ToList();
                        return View(model);
                    }

                    _logger.LogInformation("Redirecting to MedicalProfessional.");
                    return RedirectToAction("MedicalProfessionals"); // Or any other action
                }
                else
                {
                    _logger.LogWarning("Unable to retrieve logged-in user's details.");
                    ModelState.AddModelError(string.Empty, "Unable to retrieve logged-in user's details.");
                    model.Roles = GetRoles().ToList();
                    return View(model);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
                // If we got this far, something failed, redisplay form
                _logger.LogWarning("ModelState is not valid. Redisplaying form.");
                model.Roles = GetRoles().ToList(); // Ensure Roles is repopulated
                return View(model);
            }
        }

        private List<SelectListItem> GetRoles()
        {
            return _context.Roles.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.Name
            }).ToList();
        }



        private string HashPassword(string password)
        {
            // Using bcrypt to hash the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        //[HttpPost, ActionName("DeleteUser")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteUserConfirmed(int id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user != null)
        //    {
        //        _context.Users.Remove(user);
        //        await _context.SaveChangesAsync();
        //        TempData["SuccessMessage"] = "User deleted successfully.";
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = "User not found.";
        //    }
        //    return RedirectToAction(nameof(MedicalProfessionals));
        //}

        [HttpGet]
        public IActionResult EditMedicalProfessional(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to the login page or display an error message
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(MedicalProfessionals));
            }

            var viewModel = new EditRegisterViewModel
            {
                UserID = user.UserID,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                ContactNo = user.ContactNo,
                HCRNo = user.HCRNo,
                Username = user.Username


            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicalProfessional([Bind("UserID,Name,Surname,Email,ContactNo,HCRNo,Username")] EditRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Find(model.UserID);
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Surname = model.Surname;
                    user.Email = model.Email;
                    user.ContactNo = model.ContactNo;
                    user.HCRNo = model.HCRNo;
                    user.Username = model.Username;


                    try
                    {
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "User updated successfully.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating user.");
                        TempData["ErrorMessage"] = "Error updating user.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Invalid input.";
            }

            return RedirectToAction(nameof(MedicalProfessionals));
        }

        private int GetLoggedInAdminId()
        {
            // This method should retrieve the AdminID of the currently logged-in user.
            // Example implementation:
            return int.Parse(HttpContext.Session.GetString("AdminID"));
        }




        private async Task InsertRoleSpecificUser(int roleId, int userId)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
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


        private bool UserAlreadyExists(string username)
        {
            string query = "SELECT COUNT(1) FROM [User] WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        [HttpGet]
        public IActionResult MedicalProfessionals(string searchString)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewData["CurrentFilter"] = searchString;

            return View(_context.Users.Include(u => u.Roles).ToList());
        }

        public IActionResult TheatreRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var theatre = _context.Theatres.ToList();
            ViewBag.Theatre = theatre;

            return View();
        }
        public IActionResult WardRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var ward = _context.Ward.ToList();
            ViewBag.Ward = ward;

            return View();
        }
        public IActionResult ConditionRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var condition = _context.Conditions.ToList().OrderBy(c => c.Name);
            ViewBag.Condition = condition;

            return View();
        }
        public IActionResult AddCondition()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCondition(ConditionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var condition = new Condition
                    {
                        ICD_10_Code = model.ICD_10_Code,
                        Name = model.Name
                    };

                    _context.Add(condition);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Condition added successfully.");
                    return RedirectToAction("ConditionRecords", "Admin");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while adding condition: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }
        public async Task<IActionResult> EditCondition(int? id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (id == null)
            {
                return NotFound();
            }

            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }

            var viewModel = new ConditionViewModel
            {
                ConditionID = condition.ConditionID,
                ICD_10_Code = condition.ICD_10_Code,
                Name = condition.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCondition(int id, ConditionViewModel model)
        {
            if (id != model.ConditionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var condition = await _context.Conditions.FindAsync(id);
                    if (condition == null)
                    {
                        return NotFound();
                    }

                    condition.ICD_10_Code = model.ICD_10_Code;
                    condition.Name = model.Name;

                    _context.Update(condition);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Condition record updated successfully.");
                    return RedirectToAction("ConditionRecords", "Admin");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the condition record: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }
        public IActionResult AddContraIndication()
        {
            var icd = _context.ICDCodes.ToList();
            var activeIngredients = _context.Active_Ingredient.ToList();

            var icdselectList = new SelectList(icd, "ICD_ID", "Description");
            ViewBag.ICDCodes = icdselectList;

            var activeIngredientSelectList = new SelectList(activeIngredients, "Active_IngredientID", "Description");
            ViewBag.ActiveIngredients = activeIngredientSelectList;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveContraIndication(int[] selectedICD_ID, int[] selectedActive_IngredientID)
        {
            var icd = await _context.ICDCodes.Where(g => selectedICD_ID.Contains(g.ICD_ID)).ToListAsync();
            var activeIngredients = await _context.Active_Ingredient.Where(d => selectedActive_IngredientID.Contains(d.Active_IngredientID)).ToListAsync();

            foreach (var icds in icd)
            {
                foreach (var ai in activeIngredients)
                {
                    var existingEntry = await _context.Medication_Interaction.FirstOrDefaultAsync(d => d.Active_IngredientID == ai.Active_IngredientID && d.ICD_ID == icds.ICD_ID);
                    if (existingEntry != null)
                    {
                        // Update existing entry if needed
                    }
                    else
                    {
                        // Create new entry if not existent
                        _context.Medication_Interaction.Add(new Medication_Interaction { Active_IngredientID = ai.Active_IngredientID, ICD_ID = icds.ICD_ID });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ContraIndicationRecords", new { selectedICD_ID, selectedActive_IngredientID });

        }

        public async Task<IActionResult> ContraIndicationRecords()
        {

            var icdWai = await _context.ICDCodes
               .Include(icd => icd.Medication_Interaction)
                .ThenInclude(medication_interaction => medication_interaction.Active_Ingredient)
               .Select(icd => new ICD_CodesVM
               {
                   ICD_ID = icd.ICD_ID,
                   Description = icd.Description,
                   Active_Ingredient = icd.Medication_Interaction.Select(dxg => dxg.Active_Ingredient.Description).ToList()
               })
               .ToListAsync();

            return View();
        }
        public IActionResult AddMedicationInteraction()
        {
            return View();
        }
        public IActionResult MedicationInteractionRecords()
        {
            return View();
        }
    }
}
