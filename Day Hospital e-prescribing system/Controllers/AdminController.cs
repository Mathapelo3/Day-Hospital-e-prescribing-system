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


namespace Day_Hospital_e_prescribing_system.Controllers
{
    //Authorize
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<IActionResult> MedicalProfessionals(int id)
        {
            var user = await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

           
            return View(user);
        }
        public IActionResult AddMedicalProfessional()
        {
            var viewModel = new UserViewModel
            {
                Roles = _context.Role
                       .OrderBy(r => r.RoleId)
                       .Skip(1)
                       .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicalProfessional(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate Roles in case of a validation error
                model.Roles = _context.Role
                                      .OrderBy(r => r.RoleId)
                                      .Skip(1)
                                      .ToList();

                return View(model);
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Create the user object
            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                Email = model.Email,
                ContactNo = model.ContactNo,
                HCRNo = model.HCRNo,
                Username = model.Username,
                HashedPassword = hashedPassword,
                RoleId = model.RoleId,
                AdminID = GetCurrentAdminId()
            };

            // Add the user to the context
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Save to the specialization table based on role
            switch (model.RoleId)
            {
                case 2: // Assuming 2 is the RoleId for Pharmacist
                    var pharmacist = new Pharmacist { UserID = user.UserID };
                    _context.Pharmacists.Add(pharmacist);
                    break;
                case 3: // Assuming 3 is the RoleId for Surgeon
                    var surgeon = new Surgeon { UserID = user.UserID };
                    _context.Surgeons.Add(surgeon);
                    break;
                case 4: // Assuming 4 is the RoleId for Nurse
                    var nurse = new Nurse { UserID = user.UserID };
                    _context.Nurses.Add(nurse);
                    break;
                case 5: // Assuming 5 is the RoleId for Anaesthesiologist
                    var anaesthesiologist = new Anaesthesiologist { UserID = user.UserID };
                    _context.Anaesthesiologists.Add(anaesthesiologist);
                    break;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MedicalProfessionals), new { id = user.UserID });
        }

        private int GetCurrentAdminId()
        {
            //var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Get the user ID from the claims
            //var user = _context.Users.SingleOrDefault(u => u.UserID == userId); // Find the user with this user ID

            //if (user != null && user.AdminID != 0)
            //{
            //    return user.AdminID; // Return the admin ID from the user
            //}

            //throw new Exception("Current user is not associated with an admin");
            return 3;
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            var dummyUser = new User(); // Create a dummy user object just to use the PasswordHasher
            return passwordHasher.HashPassword(dummyUser, password);
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
