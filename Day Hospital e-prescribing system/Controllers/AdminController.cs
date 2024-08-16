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


namespace Day_Hospital_e_prescribing_system.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _config;
        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
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
        public async Task<IActionResult> EditHospitalRecord(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation($"Entering Edit action for ID: {id}");
            var hospitalRecord = await _context.HospitalRecords
            .Include(hr => hr.Suburb)
                .ThenInclude(s => s.City)
                .ThenInclude(c => c.Province)
            .FirstOrDefaultAsync(hr => hr.HospitalRecordID == id);

            if (hospitalRecord == null)
            {
                _logger.LogWarning("Hospital record not found.");
                return NotFound();
            }

            var suburbs = await _context.Suburbs.ToListAsync();
            var suburbList = suburbs.Select(s => new SelectListItem
            {
                Value = s.SuburbID.ToString(),
                Text = s.Name
            }).ToList();

            var viewModel = new EditHospitalRecordViewModel
            {
                HospitalRecordID = hospitalRecord.HospitalRecordID,
                Name = hospitalRecord.Name,
                AddressLine1 = hospitalRecord.AddressLine1,
                AddressLine2 = hospitalRecord.AddressLine2,
                ContactNo = hospitalRecord.ContactNo,
                Email = hospitalRecord.Email,
                PM = hospitalRecord.PM,
                PMEmail = hospitalRecord.PMEmail,
                SuburbID = hospitalRecord.SuburbID,
                SuburbName = hospitalRecord.Suburb.Name,
                PostalCode = hospitalRecord.Suburb.PostalCode,
                CityName = hospitalRecord.Suburb.City.Name,
                ProvinceName = hospitalRecord.Suburb.City.Province.Name,
                Suburbs = suburbList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalRecord(int id, EditHospitalRecordViewModel viewModel)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation($"Editing hospital record with ID: {viewModel.HospitalRecordID}");

            if (ModelState.IsValid)
            {
                var hospitalRecord = await _context.HospitalRecords
                    .Include(hr => hr.Suburb)
                    .ThenInclude(s => s.City)
                    .ThenInclude(c => c.Province)
                    .FirstOrDefaultAsync(hr => hr.HospitalRecordID == viewModel.HospitalRecordID);

                if (hospitalRecord == null)
                {
                    _logger.LogWarning("Hospital record not found.");
                    return NotFound();
                }

                // Update the HospitalRecord entity with the new values
                hospitalRecord.Name = viewModel.Name;
                hospitalRecord.AddressLine1 = viewModel.AddressLine1;
                hospitalRecord.AddressLine2 = viewModel.AddressLine2;
                hospitalRecord.ContactNo = viewModel.ContactNo;
                hospitalRecord.Email = viewModel.Email;
                hospitalRecord.PM = viewModel.PM;
                hospitalRecord.PMEmail = viewModel.PMEmail;
                hospitalRecord.SuburbID = viewModel.SuburbID;

                // Save the changes to the database
                _context.Update(hospitalRecord);
                //_context.HospitalRecords.Update(hospitalRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hospital record updated successfully!";
                return RedirectToAction("DayHospitalRecords", "Admin");
            }

            viewModel.Suburbs = await _context.Suburbs.Select(s => new SelectListItem
            {
                Value = s.SuburbID.ToString(),
                Text = s.Name
            }).ToListAsync();

            return View(viewModel);
        }
        
        public async Task<IActionResult> GetSuburbDetails(int suburbID)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var suburb = await _context.Suburbs
                .Include(s => s.City)
                .ThenInclude(c => c.Province)
                .FirstOrDefaultAsync(s => s.SuburbID == suburbID);

            if (suburb == null)
            {
                return NotFound();
            }

            var suburbDetails = new
            {
                PostalCode = suburb.PostalCode,
                CityName = suburb.City.Name,
                ProvinceName = suburb.City.Province.Name,
            };

            return Json(suburbDetails);
        }

        [HttpGet]
        public IActionResult AddMedicalProfessional()
        {
            var model = new RegisterViewModel
            {
                Roles = _context.Roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.Name
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMedicalProfessional(RegisterViewModel model)
        {
            _logger.LogInformation("Received RoleId: {RoleId}", model.RoleId);
            _logger.LogInformation("AddMedicalProfessional method started.");

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model is valid.");
                var adminId = GetLoggedInAdminId();
                var hashedPassword = HashPassword(model.Password);
                _logger.LogInformation("Hashed Password: {HashedPassword}", hashedPassword);

                var user = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    ContactNo = model.ContactNo,
                    HCRNo = model.HCRNo,
                    Username = model.Username,
                    HashedPassword = hashedPassword,
                    AdminID = adminId,
                    RoleId = model.RoleId
                };

                _logger.LogInformation("Adding new user to the database.");
                _context.Users.Add(user);
                _context.SaveChanges();
                _logger.LogInformation("User added successfully.");

                return RedirectToAction("MedicalProfessionals", "Admin");
            }
            else
            {
                _logger.LogWarning("Model is invalid.");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }

                if (model.RoleId == 0)
                {
                    ModelState.AddModelError("RoleId", "The Roles field is required.");
                }

                _logger.LogInformation("Current model values - Name: {Name}, Surname: {Surname}, Email: {Email}, ContactNo: {ContactNo}, HCRNo: {HCRNo}, Username: {Username}, RoleId: {RoleId}",
                    model.Name, model.Surname, model.Email, model.ContactNo, model.HCRNo, model.Username, model.RoleId);


                model.Roles = _context.Roles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.Name
                }).ToList();

                return View(model);
            }
        }
        // Method to hash the password
        private string HashPassword(string password)
        {
            // Using bcrypt to hash the password
            return BCrypt.Net.BCrypt.HashPassword(password);
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
                        Description = model.Description,
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
                Description = condition.Description,
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

                    condition.Description = model.Description;
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

            return RedirectToAction("ContraIndicationRecords", new { selectedICD_ID, selectedActive_IngredientID});

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
