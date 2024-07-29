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
            return View();
        }
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public async Task<IActionResult> DayHospitalRecords()
        {
            var hospitalRecords = await _context.HospitalRecords
            .Include(hr => hr.Suburb)
                .ThenInclude(s => s.City)
            .ToListAsync();

            var viewModelList = hospitalRecords.Select(hr => new HospitalRecordViewModel
            {
                HospitalRecordID = hr.HospitalRecordID,
                Name = hr.Name,
                AddressLine1 = hr.AddressLine1,
                AddressLine2 = hr.AddressLine2,
                ContactNo = hr.ContactNo,
                Email = hr.Email,
                PMContactNo = hr.PMContactNo,
                SuburbName = hr.Suburb.Name,
                PostalCode = hr.Suburb.PostalCode,
                CityName = hr.Suburb.City.Name
            }).ToList();

            return View(viewModelList);
        }
        public async Task<IActionResult> EditHospitalRecord(int id)
        {
            var hospitalRecord = await _context.HospitalRecords
            .Include(hr => hr.Suburb)
                .ThenInclude(s => s.City)
            .FirstOrDefaultAsync(hr => hr.HospitalRecordID == id);

            if (hospitalRecord == null)
            {
                return NotFound();
            }

            var suburbs = await _context.Suburbs.ToListAsync();
            var suburbList = suburbs.Select(s => new SelectListItem
            {
                Value = s.SuburbID.ToString(),
                Text = s.Name
            }).ToList();

            var viewModel = new HospitalRecordViewModel
            {
                HospitalRecordID = hospitalRecord.HospitalRecordID,
                Name = hospitalRecord.Name,
                AddressLine1 = hospitalRecord.AddressLine1,
                AddressLine2 = hospitalRecord.AddressLine2,
                ContactNo = hospitalRecord.ContactNo,
                Email = hospitalRecord.Email,
                PMContactNo = hospitalRecord.PMContactNo,
                SuburbID = hospitalRecord.SuburbID,
                SuburbName = hospitalRecord.Suburb.Name,
                PostalCode = hospitalRecord.Suburb.PostalCode,
                CityName = hospitalRecord.Suburb.City.Name,
                Suburbs = suburbList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalRecord(HospitalRecordViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var hospitalRecord = await _context.HospitalRecords
                    .Include(hr => hr.Suburb)
                        .ThenInclude(s => s.City)
                    .FirstOrDefaultAsync(hr => hr.HospitalRecordID == viewModel.HospitalRecordID);

                if (hospitalRecord == null)
                {
                    return NotFound();
                }

                hospitalRecord.Name = viewModel.Name;
                hospitalRecord.AddressLine1 = viewModel.AddressLine1;
                hospitalRecord.AddressLine2 = viewModel.AddressLine2;
                hospitalRecord.ContactNo = viewModel.ContactNo;
                hospitalRecord.Email = viewModel.Email;
                hospitalRecord.PMContactNo = viewModel.PMContactNo;
                hospitalRecord.SuburbID = viewModel.SuburbID;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Hospital record updated successfully!";
                return RedirectToAction(nameof(DayHospitalRecords));
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
            var suburb = await _context.Suburbs
                .Include(s => s.City)
                .FirstOrDefaultAsync(s => s.SuburbID == suburbID);

            if (suburb == null)
            {
                return NotFound();
            }

            var suburbDetails = new
            {
                postalCode = suburb.PostalCode,
                cityName = suburb.City.Name
            };

            return Json(suburbDetails);
        }

        [HttpGet]
        public IActionResult AddMedicalProfessional()
        {
            var roles = GetRoles();
            var model = new UserViewModel
            {
                Roles = roles
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AddMedicalProfessional(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = GetRoles();
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return View(model);
            }

            bool userExists = UserAlreadyExists(model.Username);

            if (userExists)
            {
                TempData["ErrorMessage"] = "Username already exists!";
                return View(model);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

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
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Surname", model.Surname);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@ContactNo", model.ContactNo);
                        command.Parameters.AddWithValue("@HCRNo", model.HCRNo);
                        command.Parameters.AddWithValue("@Username", model.Username);
                        command.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                        command.Parameters.AddWithValue("@AdminID", adminID);
                        command.Parameters.AddWithValue("@RoleId", model.RoleId);

                        int userId = (int)await command.ExecuteScalarAsync();

                        await InsertRoleSpecificUser(model.RoleId, userId);

                        TempData["SuccessMessage"] = "User successfully added into the system.";
                        return RedirectToAction("MedicalProfessionals");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the user: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(model);
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
        public async Task<IActionResult> MedicalProfessionals(int id)
        {
            List<UserViewModel> medicalProfessionals = new List<UserViewModel>();

            string query = "SELECT u.UserID, u.Name, u.Surname, u.Email, u.ContactNo, u.HCRNo, u.Username, r.Name AS Role " +
                           "FROM [User] u " +
                           "JOIN [Role] r ON u.RoleId = r.RoleId";

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                medicalProfessionals.Add(new UserViewModel
                                {
                                    UserID = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Surname = reader.GetString(2),
                                    Email = reader.GetString(3),
                                    ContactNo = reader.GetString(4),
                                    HCRNo = reader.GetString(5),
                                    Username = reader.GetString(6),
                                    Role = reader.GetString(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return View(medicalProfessionals);
        }

        public IActionResult TheatreRecords()
        {
            var theatre = _context.Theatres.ToList();
            ViewBag.Theatre = theatre;

            return View();
        }
        public IActionResult WardRecords()
        {
            var ward = _context.Wards.ToList();
            ViewBag.Ward = ward;

            return View();
        }
        public IActionResult ConditionRecords()
        {
            var condition = _context.Conditions.ToList().OrderBy(c => c.Name);
            ViewBag.Condition = condition;

            return View();
        }
        public IActionResult AddCondition()
        {
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
