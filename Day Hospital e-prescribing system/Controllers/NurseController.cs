using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication27.Models;
using static Day_Hospital_e_prescribing_system.ViewModel.DisplayVitalsVM;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class NurseController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<NurseController> _logger;
        private readonly IConfiguration _config;

        public NurseController(ApplicationDbContext context, ILogger<NurseController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }
        public IActionResult Dashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public IActionResult UpdatePatientInfo()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        // GET: Vitals/Add
        public async Task<ActionResult> Vitals(int selectedId)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (selectedId <= 0)
            {
                _logger.LogWarning("Invalid admission id: {Id}", selectedId);
                return NotFound("Invalid admission ID.");
            }

            var selectedAdmission = await _context.Surgeries
                .Include(a => a.Patients)
                .FirstOrDefaultAsync(a => a.SurgeryID == selectedId);

            if (selectedAdmission == null)
            {
                _logger.LogWarning("Admission not found with id: {Id}", selectedId);
                return NotFound("Admission not found.");
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);


            var vitalsList = await _context.Vitals
       .Select(v => new Patient_VitalsVM
       {
           Vital = v.Vital,
           Min = v.Min,
           Max = v.Max,
           Notes = string.Empty
       }).ToListAsync();


            var model = new VitalsVM
            {
                //SurgeryID = selectedAdmission.SurgeryID,
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = DateTime.Now.Date,
                Time = DateTime.Now.TimeOfDay,
                Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight,
                Height = patient.Patient_Vitals.FirstOrDefault()?.Height,
                Vitals = vitalsList // Populate with the available vitals
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Vitals(VitalsVM model)
        {

            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Starting to capture vitals...");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid. Errors: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return View(model);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("User ID not found in claims.");
                ModelState.AddModelError("", "User ID not found in claims");
                return View(model);
            }

            int userId = int.Parse(userIdClaim.Value);
            var nurseIdClaim = User.Claims.FirstOrDefault(c => c.Type == "NurseID");
            if (nurseIdClaim == null)
            {
                _logger.LogError("Nurse ID not found in claims.");
                ModelState.AddModelError("", "Nurse ID not found in claims");
                return View(model);
            }

            int nurseId = int.Parse(nurseIdClaim.Value);

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == model.PatientID);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found with ID: {id}", model.PatientID);
                return NotFound();
            }

            // Create a list to hold the vitals entries
            var patientVitalsList = new List<Patient_Vitals>();

            foreach (var vitalVM in model.Vitals)
            {
                var vitalId = _context.Vitals
                      .Where(v => v.Vital == vitalVM.Vital)
                      .Select(v => v.VitalsID)
                      .FirstOrDefault(); // Assuming Vital is unique

                // Check if the vitalId is valid before proceeding
                if (vitalId == 0)
                {
                    _logger.LogWarning($"VitalsID not found for Vital: {vitalVM.Vital}");
                    continue; // Skip this iteration if the VitalsID is invalid
                }
                var vital = new Patient_Vitals
                {
                    PatientID = model.PatientID,
                    //VitalsID = vitalVM.VitalsID,
                    VitalsID = vitalId,
                    Date = model.Date,
                    Time = model.Time,
                    Value = vitalVM.Value,
                    Notes = vitalVM.Notes,
                    Weight = model.Weight,
                    Height = model.Height
                };

                // Log the vital information being added
                _logger.LogInformation($"Adding vital for PatientID: {model.PatientID}, VitalID: {vital.VitalsID}, Value: {vital.Value}");

                patientVitalsList.Add(vital);
            }

            _context.Patient_Vitals.AddRange(patientVitalsList);
            await _context.SaveChangesAsync();

            return RedirectToAction("DisplayVitals", "Nurse");
        }

        public async Task<ActionResult> DisplayVitals(int selectedId)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");


            var selectedSurgery = await _context.Surgeries
                .Include(a => a.Patients)
                .FirstOrDefaultAsync(a => a.SurgeryID == selectedId);
            if (selectedSurgery == null)
            {
                return NotFound("Admission not found.");
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedSurgery.PatientID);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            var firstVitals = patient.Patient_Vitals.FirstOrDefault();

            var model = new VitalsVM
            {
                //SurgeryID = selectedSurgery.SurgeryID,
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = firstVitals?.Date ?? DateTime.Now,
                Time = firstVitals?.Time ?? TimeSpan.Zero,
                Height = firstVitals?.Height,
                Weight = firstVitals?.Weight,
                Vitals = patient.Patient_Vitals.Select(pv => new Patient_VitalsVM
                {
                    VitalsID = pv.VitalsID,
                    Vital = pv.Vitals?.Vital,  // Use null conditional operator
                    Value = pv.Value,
                    Date = pv.Date,
                    /*  Time = pv.Time.HasValue ? pv.Time.Value.ToString(@"hh\:mm") : null,*/  // Check if Time is not null
                    Notes = pv.Notes
                }).ToList()
            };

            return View(model);

        }
        public async Task<ActionResult> PatientsVitals()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

          

            //if (patient == null)
            //{
            //    return NotFound();
            //}

            var surgeries = _context.Surgeries
                             .Include(s => s.Patients)
                             .Include(s => s.Theatres)
                             .Include(s => s.Anaesthesiologists)
                             //.Include(s => s.Surgery_TreatmentCodes)
                             .Select(s => new BookedDetailsVM
                             {
                                 SurgeryID = s.SurgeryID,
                                 PatientID = s.PatientID,
                                 AnaesthesiologistID = s.AnaesthesiologistID,
                                 TheatreID = s.TheatreID,
                                 //Surgery_TreatmentCodeID = s.Surgery_TreatmentCodeID,
                                 //ICD_Code_10 = s.Surgery_TreatmentCodes.ICD_10_Code,
                                 PatientName = s.Patients.Name,
                                 PatientSurname = s.Patients.Surname,
                                 TheatreName = s.Theatres.Name,
                                 AnaesthesiologistName = s.Anaesthesiologists.User.Name,
                                 AnaesthesiologistSurname = s.Anaesthesiologists.User.Surname,
                                 //Date = s.Date,
                                 Time = s.Time // Assuming these properties exist on your Surgery entity
                             })
                              .ToList();

            return View(surgeries);
        }
        public IActionResult VitalsEdit()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public async Task<ActionResult> PatientsPrescripstions(string searchString)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewData["CurrentFilter"] = searchString;

            var patientAd = _context.Surgeries
                                       .Include(s => s.Patients)
                                        .ThenInclude(s => s.Beds)
                                       //.Include(s => s.Theatres)
                                       .Include(s => s.Surgeons)
                                       //.Include(s => s.Surgery_TreatmentCodes)
                                       .Select(s => new AdmissionVM
                                       {
                                           SurgeryID = s.SurgeryID,
                                           PatientID = s.PatientID,
                                           //AnaesthesiologistID = s.AnaesthesiologistID,
                                           //TheatreID = s.TheatreID,
                                           Gender=s.Patients.Gender,
                                           BedName = s.Patients.Beds.BedName,
                                           WardName = s.Patients.Beds.Wards.WardName,
                                           //Surgery_TreatmentCodeID = s.Surgery_TreatmentCodeID,
                                           //ICD_Code_10 = s.Surgery_TreatmentCodes.ICD_10_Code,
                                           Name = s.Patients.Name,
                                           Surname = s.Patients.Surname,
                                           //TheatreName = s.Theatres.Name,
                                           SurgeonName = s.Surgeons.User.Name,
                                           SurgeonSurname = s.Surgeons.User.Surname,
                                           //Date = s.Date,
                                           //Time = s.Time // Assuming these properties exist on your Surgery entity
                                       })
                                        .ToList();


            return View(patientAd);
        }
        public IActionResult Prescription()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
          
            var patientPrescript = _context.Prescriptions
                             .Include(s => s.Patient)
                              .Include(s => s.Surgeon)
                               .Include(s => s.Medication)
                             .Select(s => new PrescriptionVM
                             {
                                 PrescriptionID = s.PrescriptionID,
                                 PatientID = s.PatientID,
                                 SurgeonID = s.SurgeonID,
                                 MedicationID = s.MedicationID,
                                 Name = s.Patient.Name,
                                 Surname = s.Patient.Surname,
                                 SurgeonName = s.Surgeon.User.Name,
                                 SurgeonSurname = s.Surgeon.User.Surname,
                                 Instruction =s.Instruction,
                                 Medication = s.Medication.Name,
                                 Status = s.Status,
                                 Date = s.Date,
                                 Quantity = s.Quantity 
                             })
                              .ToList();

            return View(patientPrescript);
        }

        public async Task<ActionResult> Discharge(int id)
        {
            try
            {
                var patient = await GetPatientByIdAsync(id);
                if (patient == null)
                {
                    return NotFound();
                }

                var patientVM = new PatientVM
                {
                    PatientID = patient.PatientID,
                    Name = patient.Name,
                    Surname = patient.Surname,
                    Gender = patient.Gender,
                    DateOfBirth = patient.DateOfBirth,
                    IDNo = patient.IDNo,
                    Email = patient.Email,
                    SuburbID = patient.SuburbID,
                    AddressLine1 = patient.AddressLine1,
                    AddressLine2 = patient.AddressLine2,
                    ContactNo = patient.ContactNo,
                    NextOfKinNo = patient.NextOfKinNo,
                    // Map other properties as needed
                };

                await PopulateDropdowns(patient.SuburbID);
                await PopulateDropdowns(patientVM.CityID);
                await PopulateDropdowns(patientVM.ProvinceID);
                return View(patientVM);
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Error in Discharge GET: {ex.Message}");
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int id, PatientVM patientVM)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (ModelState.IsValid)
            {
                try
                {

                    if (id != patientVM.PatientID)
                    {
                        return NotFound();
                    }

                    var patient = new Patient
                    {

                        Name = patientVM.Name,
                        Surname = patientVM.Surname,
                        Gender = patientVM.Gender,
                        DateOfBirth = patientVM.DateOfBirth,
                        IDNo = patientVM.IDNo,
                        Email = patientVM.Email,
                        SuburbID = patientVM.SuburbID,
                        AddressLine1 = patientVM.AddressLine1,
                        AddressLine2 = patientVM.AddressLine2,
                        ContactNo = patientVM.ContactNo,
                        NextOfKinNo = patientVM.NextOfKinNo,

                    };

                    await UpdatePatientAsync(patient);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Patient info updated successfully with id: {Id}", id);
                    return RedirectToAction(nameof(Discharge), new { id = patient.PatientID });
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    _logger.LogError(ex, "Concurrency error occurred while updating order with id: {Id}", id);
                    throw;
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }
                _logger.LogWarning("ModelState is invalid.");
            }

            await PopulateDropdowns(patientVM.SuburbID);
            await PopulateDropdowns(patientVM.CityID);
            await PopulateDropdowns(patientVM.ProvinceID);
            return View(patientVM);
        }
        private async Task<Patient> GetPatientByIdAsync(int id)
        {
            const string query = @"
            SELECT p.PatientID, p.Name, p.Surname, p.DateOfBirth, p.IDNo, p.Gender, 
                   p.AddressLine1, p.AddressLine2, p.Email, p.ContactNo, p.NextOfKinNo, 
                   p.SuburbID, s.CityID, s.PostalCode, c.ProvinceID
            FROM Patient p
            INNER JOIN Suburb s ON p.SuburbID = s.SuburbID
            INNER JOIN City c ON s.CityID = c.CityID
            WHERE p.PatientID = @PatientID";

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PatientID", id);
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Patient
                        {
                            PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Surname = reader.GetString(reader.GetOrdinal("Surname")),
                            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                            IDNo = reader.GetString(reader.GetOrdinal("IDNo")),
                            Gender = reader.GetString(reader.GetOrdinal("Gender")),
                            AddressLine1 = reader.GetString(reader.GetOrdinal("AddressLine1")),
                            AddressLine2 = reader.IsDBNull(reader.GetOrdinal("AddressLine2")) ? null : reader.GetString(reader.GetOrdinal("AddressLine2")),
                            Email = reader.GetString(reader.GetOrdinal("Email")),
                            ContactNo = reader.GetString(reader.GetOrdinal("ContactNo")),
                            NextOfKinNo = reader.GetString(reader.GetOrdinal("NextOfKinNo")),
                            SuburbID = reader.GetInt32(reader.GetOrdinal("SuburbID")),
                            Suburbs = new Suburb
                            {
                                SuburbID = reader.GetInt32(reader.GetOrdinal("SuburbID")),
                                CityID = reader.GetInt32(reader.GetOrdinal("CityID")),
                                PostalCode = reader.GetString(reader.GetOrdinal("PostalCode")),
                                City = new City
                                {
                                    CityID = reader.GetInt32(reader.GetOrdinal("CityID")),
                                    ProvinceID = reader.GetInt32(reader.GetOrdinal("ProvinceID")),
                                    Province = new Province
                                    {
                                        ProvinceID = reader.GetInt32(reader.GetOrdinal("ProvinceID"))
                                    }
                                }
                            }
                        };
                    }
                }
            }
            return null;
        }

        private async Task UpdatePatientAsync(Patient patient)
        {
            const string updateQuery = @"
            UPDATE Patient 
            SET Name = @Name, Surname = @Surname, DateOfBirth = @DateOfBirth, 
                IDNo = @IDNo, Gender = @Gender, AddressLine1 = @AddressLine1, 
                AddressLine2 = @AddressLine2, Email = @Email, ContactNo = @ContactNo, 
                NextOfKinNo = @NextOfKinNo, SuburbID = @SuburbID
            WHERE PatientID = @PatientID";

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", patient.Name);
                command.Parameters.AddWithValue("@Surname", patient.Surname);
                command.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
                command.Parameters.AddWithValue("@IDNo", patient.IDNo);
                command.Parameters.AddWithValue("@Gender", patient.Gender);
                command.Parameters.AddWithValue("@AddressLine1", patient.AddressLine1);
                command.Parameters.AddWithValue("@AddressLine2", (object)patient.AddressLine2 ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", patient.Email);
                command.Parameters.AddWithValue("@ContactNo", patient.ContactNo);
                command.Parameters.AddWithValue("@NextOfKinNo", patient.NextOfKinNo);
                command.Parameters.AddWithValue("@SuburbID", patient.SuburbID);
                command.Parameters.AddWithValue("@PatientID", patient.PatientID);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task PopulateDropdowns(int suburbId)
        {
            try
            {
                // Get the suburb details including city and province
                const string query = @"
            SELECT s.SuburbID, s.CityID, c.ProvinceID
            FROM Suburb s
            INNER JOIN City c ON s.CityID = c.CityID
            WHERE s.SuburbID = @SuburbID";

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SuburbID", suburbId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int cityId = reader.GetInt32(reader.GetOrdinal("CityID"));
                                int provinceId = reader.GetInt32(reader.GetOrdinal("ProvinceID"));

                                // Populate dropdowns
                                ViewBag.Provinces = await GetProvincesAsync();
                                ViewBag.Cities = await GetCitiesByProvinceAsync(provinceId);
                                ViewBag.Suburbs = await GetSuburbsByCityAsync(cityId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Debug.WriteLine($"Error in PopulateDropdowns: {ex.Message}");
                // You might want to throw the exception here or handle it appropriately
            }
        }

        private async Task<List<SelectListItem>> GetProvincesAsync()
        {
            const string query = "SELECT ProvinceID, Name FROM Province";
            var provinces = new List<SelectListItem>();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        provinces.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("ProvinceID")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }

            return provinces;
        }

        private async Task<List<SelectListItem>> GetCitiesByProvinceAsync(int provinceId)
        {
            const string query = "SELECT CityID, Name FROM City WHERE ProvinceID = @ProvinceID";
            var cities = new List<SelectListItem>();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProvinceID", provinceId);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cities.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("CityID")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }

            return cities;
        }

        private async Task<List<SelectListItem>> GetSuburbsByCityAsync(int cityId)
        {
            const string query = "SELECT SuburbID, Name, PostalCode FROM Suburb WHERE CityID = @CityID";
            var suburbs = new List<SelectListItem>();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CityID", cityId);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        suburbs.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("SuburbID")).ToString(),
                            Text = $"{reader.GetString(reader.GetOrdinal("Name"))} ({reader.GetString(reader.GetOrdinal("PostalCode"))})"
                        });
                    }
                }
            }

            return suburbs;
        }

        private async Task<int> GetProvinceIdBySuburbAsync(int suburbId)
        {
            const string query = @"
            SELECT c.ProvinceID
            FROM Suburb s
            INNER JOIN City c ON s.CityID = c.CityID
            WHERE s.SuburbID = @SuburbID";

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SuburbID", suburbId);
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return (int)result;
            }
        }

        private async Task<int> GetCityIdBySuburbAsync(int suburbId)
        {
            const string query = "SELECT CityID FROM Suburb WHERE SuburbID = @SuburbID";

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SuburbID", suburbId);
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return (int)result;
            }
        }

        // GET: Nurse/GetCitiesByProvince/5
        [HttpGet]
        public async Task<JsonResult> GetCitiesByProvince(int provinceId)
        {
            var cities = await GetCitiesByProvinceAsync(provinceId);
            return Json(cities);
        }

        // GET: Nurse/GetSuburbsByCity/5
        [HttpGet]
        public async Task<JsonResult> GetSuburbsByCity(int cityId)
        {
            var suburbs = await GetSuburbsByCityAsync(cityId);
            return Json(suburbs);
        }

        [HttpGet]
        public async Task<ActionResult> PatientsWChronics(string searchString)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;

            var patient = from p in _context.Patients
                          select new Patient
                          {
                              PatientID = p.PatientID,
                              Name = p.Name ?? string.Empty,
                              Surname = p.Surname ?? string.Empty,
                              Email = p.Email ?? string.Empty,
                              IDNo = p.IDNo ?? string.Empty,
                              Gender = p.Gender ?? string.Empty,
                              Status = p.Status ?? string.Empty
                          };

            if (!String.IsNullOrEmpty(searchString))
            {
                patient = patient.Where(p => p.IDNo.Contains(searchString));
            }

            return View(await patient.ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult> Codition2(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var patient = await _context.Patients
               .Where(p => p.PatientID == id)
               .Select(p => new AddConditionsVM
               {
                   PatientID = p.PatientID,
                   Name = p.Name,
                   Surname = p.Surname,
               })
        .FirstOrDefaultAsync();

            if (patient == null)
            {
                // Handle the case where the patient was not found
                return NotFound();
            }


            await ReloadDropdownData(patient);
            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Codition2(AddConditionsVM model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToArray();

                foreach (var error in errors)
                {
                    _logger.LogError($"Error in {error.Key}: {string.Join(", ", error.Errors.Select(e => e.ErrorMessage))}");
                }

                // Reload patient data
                var patient = await _context.Patients
                    .Where(p => p.PatientID == model.PatientID)
                    .Select(p => new { p.Name, p.Surname })
                    .FirstOrDefaultAsync();

                if (patient != null)
                {
                    model.Name = patient.Name;
                    model.Surname = patient.Surname;

                }

                await ReloadDropdownData(model);
                return View(model);
            }

            try
            {
                _logger.LogInformation($"Attempting to add condition details: PatientID={model.PatientID}, Condition={model.SelectedCondition}, Allergy={model.SelectedAllergy}, Medication={model.SelectedMedication}");

                var successParam = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                // Convert SelectedCondition to a name if it's an ID
                string conditionName = await GetConditionNameById(model.SelectedCondition);
                string allergyName = await GetAllergyNameById(model.SelectedAllergy);
                string medicationName = await GetMedicationNameById(model.SelectedMedication);

                var parameters = new[]
                {
            new SqlParameter("@PatientID", SqlDbType.Int) { Value = model.PatientID },
            new SqlParameter("@ConditionName", SqlDbType.NVarChar, 50) { Value = (object)conditionName ?? DBNull.Value },
            new SqlParameter("@AllergyName", SqlDbType.NVarChar, 50) { Value = (object)allergyName ?? DBNull.Value },
            new SqlParameter("@MedicationName", SqlDbType.NVarChar, 50) { Value = (object)medicationName ?? DBNull.Value },
            successParam,
            errorMessageParam
        };

                var sql = "EXEC dbo.AddConditionDetails @PatientID, @ConditionName, @AllergyName, @MedicationName, @Success OUTPUT, @ErrorMessage OUTPUT";

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                bool success = (bool)successParam.Value;
                string errorMessage = errorMessageParam.Value as string;

                _logger.LogInformation($"AddConditionDetails procedure executed. Success: {success}, ErrorMessage: {errorMessage}");

                if (success)
                {
                    TempData["SuccessMessage"] = "Condition details added successfully.";
                    return RedirectToAction("Codition2", new { id = model.PatientID });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Failed to add condition details: {errorMessage}");
                    _logger.LogError($"Failed to add condition details for PatientID: {model.PatientID}. Error: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while adding condition details for PatientID: {model.PatientID}");
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
            }

            // Reload patient data
            var patientData = await _context.Patients
                .Where(p => p.PatientID == model.PatientID)
                .Select(p => new { p.Name, p.Surname })
                .FirstOrDefaultAsync();

            if (patientData != null)
            {
                model.Name = patientData.Name;
                model.Surname = patientData.Surname;
            }

            await ReloadDropdownData(model);
            return View(model);
        }
    
        private async Task ReloadDropdownData(AddConditionsVM model)
        {
            model.Condition = await GetConditionsAsync();
            model.Allergy = await GetAllergiesAsync();
            model.General_Medication = await GetMedicationsAsync();
        }
        private async Task<string> GetConditionNameById(string selectedCondition)
        {
            if (int.TryParse(selectedCondition, out int conditionId))
            {
                var condition = await _context.Conditions.FindAsync(conditionId);
                return condition?.Name;
            }
            return selectedCondition;
        }

        private async Task<string> GetAllergyNameById(string selectedAllergy)
        {
            if (int.TryParse(selectedAllergy, out int allergyId))
            {
                var allergy = await _context.Allergies.FindAsync(allergyId);
                return allergy?.Name;
            }
            return selectedAllergy;
        }

        private async Task<string> GetMedicationNameById(string selectedMedication)
        {
            if (int.TryParse(selectedMedication, out int medicationId))
            {
                var medication = await _context.General_Medication.FindAsync(medicationId);
                return medication?.Name;
            }
            return selectedMedication;
        }

        private async Task<List<SelectListItem>> GetConditionsAsync()
        {
            var conditions = new List<SelectListItem>();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT ConditionID, Name FROM Condition", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            conditions.Add(new SelectListItem
                            {
                                Value = reader["ConditionID"].ToString(),
                                Text = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return conditions;
        }

        private async Task<List<SelectListItem>> GetAllergiesAsync()
        {
            var allergies = new List<SelectListItem>();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT AllergyID, Name FROM Allergy", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            allergies.Add(new SelectListItem
                            {
                                Value = reader["AllergyID"].ToString(),
                                Text = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return allergies;
        }

        private async Task<List<SelectListItem>> GetMedicationsAsync()
        {
            var medications = new List<SelectListItem>();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT General_MedicationID, Name FROM General_Medication", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            medications.Add(new SelectListItem
                            {
                                Value = reader["General_MedicationID"].ToString(),
                                Text = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return medications;
        }

        [HttpGet]
        public async Task<ActionResult> AddCodition2(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            //var selectedAdmission = await _context.Surgeries
            //    .Include(a => a.Patients)
            //    .FirstOrDefaultAsync(a => a.SurgeryID == selectedId);

            if (id == null)
            {
                return NotFound("Patient not found.");
            }

            var patient = await _context.Patients
                 .Include(p => p.Patient_Allergy)
                 .ThenInclude(pa => pa.Allergy)
                 .Include(p => p.Patient_Condition)
                 .ThenInclude(pc => pc.Condition)
                 .Include(p => p.Patient_Medication)
                 .ThenInclude(pm => pm.General_Medication)
                 .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }
            // Debugging step: check the retrieved conditions
            var conditions = patient.Patient_Condition.Select(pc => pc.Condition).ToList();
            if (!conditions.Any())
            {
                // No conditions found, handle accordingly
                Debug.WriteLine("No conditions found for the patient.");
            }
            var allergies = patient.Patient_Allergy.Select(pc => pc.Allergy).ToList();
            if (!conditions.Any())
            {
                // No conditions found, handle accordingly
                Debug.WriteLine("No allergies found for the patient.");
            }
            var meds = patient.Patient_Medication.Select(pc => pc.General_Medication).ToList();
            if (!conditions.Any())
            {
                // No conditions found, handle accordingly
                Debug.WriteLine("No medications found for the patient.");
            }


            var viewModel = new ConditionsVM
            {
                //SurgeryID = selectedAdmission.SurgeryID,
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Allergies = patient.Patient_Allergy.Select(pa => pa.Allergy.Name).ToList(),
                Conditions = patient.Patient_Condition.Select(pc => pc.Condition.Name).ToList(),
                General_Medications = patient.Patient_Medication.Select(pm => pm.General_Medication.Name).ToList(),

            };
            return View(viewModel);



        }
        public IActionResult DisplayAdmission()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> AdmissionWizard(DateTime? startDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            //var patient = _context.Surgeries.FirstOrDefault(p => p.SurgeryID == id);

            //if (patient == null)
            //{
            //    return NotFound();
            //}

            var surgeries = _context.Surgeries
                             .Include(s => s.Patients)
                             .Include(s => s.Theatres)
                             .Include(s => s.Anaesthesiologists)
                             //.Include(s => s.Surgery_TreatmentCodes)
                             .Select(s => new BookedDetailsVM
                             {
                                 SurgeryID = s.SurgeryID,
                                 PatientID = s.PatientID,
                                 AnaesthesiologistID = s.AnaesthesiologistID,
                                 TheatreID = s.TheatreID,
                                 //Surgery_TreatmentCodeID = s.Surgery_TreatmentCodeID,
                                 //ICD_Code_10 = s.Surgery_TreatmentCodes.ICD_10_Code,
                                 PatientName = s.Patients.Name,
                                 PatientSurname = s.Patients.Surname,
                                 TheatreName = s.Theatres.Name,
                                 AnaesthesiologistName = s.Anaesthesiologists.User.Name,
                                 AnaesthesiologistSurname = s.Anaesthesiologists.User.Surname,
                                 //Date = s.Date,
                                 Time = s.Time // Assuming these properties exist on your Surgery entity
                             })
                              .ToList();

            // Apply date filtering
            if (startDate.HasValue)
            {
                surgeries = (List<BookedDetailsVM>)surgeries.Where(s => s.Date.Date == startDate.Value.Date);
            }

            //var surgeries = await surgeries.ToListAsync();

            return View(surgeries);
        }
        [HttpGet]
        public async Task<ActionResult> AdministerMeds(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("GET AdministerMeds called for PatientID: {PatientID}", id);

            try
            {
                //var model = new AdministerMedicationVM();
                var model = new AdministerMedicationVM();

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Get patient information
                    using (var command = new SqlCommand("sp_GetPatientInfo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientID", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.PatientID = id;
                                model.Name = reader["Name"].ToString();
                                model.Surname = reader["Surname"].ToString();
                                _logger.LogInformation("Patient info retrieved: ID {PatientID}, Name {Name} {Surname}", id, model.Name, model.Surname);

                            }
                            else
                            {
                                _logger.LogWarning("Patient not found for ID: {PatientID}", id);
                                return NotFound("Patient not found.");
                            }
                        }
                    }

                    // Get medications and quantity
                    using (var command = new SqlCommand("sp_GetPrescriptionMeds", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientID", id); // Ensure the patient ID is passed to this stored procedure

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.PrescriptionID = (int)reader["PrescriptionID"];
                                model.Medication = reader["Medication"]?.ToString() ?? string.Empty;
                                model.Quantity = reader["Quantity"]?.ToString() ?? string.Empty;
                                _logger.LogInformation("Prescription info retrieved: ID {PrescriptionID}, Medication {Medication}, Quantity {Quantity}",
                                    model.PrescriptionID, model.Medication, model.Quantity);
                            }
                            else
                            {
                                _logger.LogWarning("No prescription found for PatientID: {PatientID}", id);
                                return RedirectToAction("NoPrescription", new { patientId = id });
                            }
                        }
                    }

                    //await connection.OpenAsync();
                    //using (var reader = await command.ExecuteReaderAsync())
                    //{
                    //    if (await reader.ReadAsync())
                    //    {
                    //        model.PrescriptionID = (int)reader["PrescriptionID"];
                    //        model.Medication = reader["Medication"]?.ToString() ?? string.Empty;
                    //        model.Quantity = reader["Quantity"]?.ToString() ?? string.Empty;
                    //    }
                    //    else
                    //    {
                    //        return RedirectToAction("NoPrescription", new { patientId = id });
                    //    }
                    //}


                    model.Date = DateTime.Now.Date;
                model.Time = DateTime.Now.TimeOfDay;

                return View(model);
            }
                }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while retrieving patient data for ID: {PatientID}", id);
                return StatusCode(500, "An error occurred while retrieving patient data.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdministerMeds(AdministerMedicationVM model)
        {

            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("POST AdministerMeds called with PatientID: {PatientID}, PrescriptionID: {PrescriptionID}",
       model.PatientID, model.PrescriptionID);


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for AdministerMeds");
                return View(model);
            }

            if (model.PrescriptionID <= 0)
            {
                _logger.LogWarning("Invalid PrescriptionID: {PrescriptionID}", model.PrescriptionID);
                ModelState.AddModelError(string.Empty, "Invalid prescription. Please try again.");
                return View(model);
            }

            // In your POST method, before inserting
            if (!await IsPrescriptionValidForPatient(model.PatientID, model.PrescriptionID))
            {
                _logger.LogWarning("Invalid prescription for patient. PatientID: {PatientID}, PrescriptionID: {PrescriptionID}",
                    model.PatientID, model.PrescriptionID);
                ModelState.AddModelError(string.Empty, "The selected prescription is not valid for this patient.");
                return View(model);
            }

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("InsertAdministerMedication", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Date", model.Date);
                        command.Parameters.AddWithValue("@Time", model.Time);
                        command.Parameters.AddWithValue("@Administer", model.Administer);
                        command.Parameters.AddWithValue("@PrescriptionID", model.PrescriptionID);
                        command.Parameters.AddWithValue("@NurseID", model.NurseID);

                        _logger.LogInformation("Executing InsertAdministerMedication with PrescriptionID: {PrescriptionID}", model.PrescriptionID);
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Successfully administered medication for PatientID: {PatientID}, PrescriptionID: {PrescriptionID}",
                            model.PatientID, model.PrescriptionID);
                    }
                }
                

                return RedirectToAction("AdministeredMeds", new { model.PatientID });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while saving medication administration data. PatientID: {PatientID}, PrescriptionID: {PrescriptionID}",
            model.PatientID, model.PrescriptionID);
                ModelState.AddModelError(string.Empty, "An error occurred while saving data. Please try again.");
                return View(model);
            }
           
        }
        //Add this method to your controller
        private async Task<bool> IsPrescriptionValidForPatient(int patientId, int prescriptionId)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Prescription WHERE PrescriptionID = @PrescriptionID", connection))
                {
                    //command.Parameters.AddWithValue("@PatientID", patientId);
                    command.Parameters.AddWithValue("@PrescriptionID", prescriptionId);
                    var result = (int)await command.ExecuteScalarAsync() > 0;
                    _logger.LogInformation("Prescription validation result: {Result}. PatientID: {PatientID}, PrescriptionID: {PrescriptionID}",
                        result, patientId, prescriptionId);
                    return result;
                }
            }
        }


        [HttpGet]
        public async Task<ActionResult> RetakeVitals2(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");


            try
            {
                var model = new VitalsVM();

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Get patient information
                    using (var command = new SqlCommand("sp_GetPatientInfo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientID", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.PatientID = id;
                                model.Name = reader["Name"].ToString();
                                model.Surname = reader["Surname"].ToString();
                                model.Weight = reader["Weight"] != DBNull.Value ? (string?)reader["Weight"] : null;
                                model.Height = reader["Height"] != DBNull.Value ? (string?)reader["Height"] : null;
                            }
                            else
                            {
                                return NotFound("Patient not found.");
                            }
                        }
                    }

                    // Get vitals list
                    model.Vitals = new List<Patient_VitalsVM>();
                    using (var command = new SqlCommand("sp_GetVitalsList", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                model.Vitals.Add(new Patient_VitalsVM
                                {
                                    Vital = reader["Vital"].ToString(),
                                    Min = (string)reader["Min"],
                                    Max = (string)reader["Max"],
                                    Value = string.Empty,
                                    Notes = string.Empty
                                });
                            }
                        }
                    }
                }

                model.Date = DateTime.Now.Date;
                model.Time = DateTime.Now.TimeOfDay;

                return View(model);
            }
            catch (SqlException ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while retrieving patient data.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RetakeVitals2(VitalsVM model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (!ModelState.IsValid)
            {
                // Handle invalid model state
                return View(model);
            }

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("InsertRetakeVitals", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@PatientID", model.PatientID);
                        command.Parameters.AddWithValue("@Date", model.Date);
                        command.Parameters.AddWithValue("@Time", model.Time);
                        command.Parameters.AddWithValue("@Notes", model.Notes);
                        command.Parameters.AddWithValue("@VitalsData", JsonConvert.SerializeObject(model.Vitals));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Process the results if needed
                            while (await reader.ReadAsync())
                            {
                                // Read and process each row
                            }
                        }
                    }
                }

                return RedirectToAction("DisplayVitalsPerPatient", new { model.PatientID });
            }
            catch (SqlException ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, "An error occurred while saving vitals data.");
                return View(model);
            }
        }
    
        public IActionResult RetakeVitals3()
        {
            return View();
        }
        public IActionResult DisplayVitals3()
        {
            return View();
        }
        public IActionResult AdministeredMeds(int id, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var patientPrescript = _context.Administer_Medication
                             .Include(s => s.Prescription)
                              .ThenInclude(s => s.Patient)
                             .Select(s => new AdministeredMedsVM
                             {
                                 PrescriptionID = s.PrescriptionID,
                                 PatientID = s.Prescription.Patient.PatientID,
                                 MedicationID = s.Prescription.MedicationID,
                                 Name = s.Prescription.Patient.Name,
                                 Surname = s.Prescription.Patient.Surname,
                                 Instruction = s.Prescription.Instruction,
                                 Medication = s.Prescription.Medication.Name,
                                 Time = s.Time,
                                 Date = s.Date,
                                 Quantity = s.Prescription.Quantity,
                                 Administer = s.Administer
                             })
                              .ToList();

            // Apply date filtering in memory
            if (startDate.HasValue)
            {
                patientPrescript = patientPrescript.Where(s => s.Date >= startDate.Value.Date).ToList();
            }
            if (endDate.HasValue)
            {
                patientPrescript = patientPrescript.Where(s => s.Date <= endDate.Value.Date).ToList();
            }

            // Pass the date values to the view for maintaining filter state
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(patientPrescript);
        }
        public async Task<ActionResult> AdmittedPatients()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");



            var patientAdmission = from p in _context.Patients
                                   select new Patient
                                   {
                                       PatientID = p.PatientID,
                                       Name = p.Name ?? string.Empty,
                                       Surname = p.Surname ?? string.Empty,
                                       ContactNo = p.ContactNo ?? string.Empty,
                                       IDNo = p.IDNo ?? string.Empty,
                                       Gender = p.Gender ?? string.Empty,

                                   };


            return View(patientAdmission);
        }

        public List<SelectListItem> GetWard()
        {
            List<SelectListItem> ward = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT WardId, WardName FROM [Ward]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ward.Add(new SelectListItem
                                {
                                    Value = reader["WardId"].ToString(),
                                    Text = reader["WardName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Failed to fetch ward: {ex.Message}");
            }

            return ward;
        }
        [HttpGet]
        public async Task<IActionResult> AdmitPatient(int selectedId)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            //var treatmentCode = GetTreatmentCode();

            var treatmentCode = GetTreatmentCode();

            var selectedSurgery = await _context.Surgeries
                                .Include(s => s.Patients)
                                //.Include(s => s.Surgery_TreatmentCodes)
                                .DefaultIfEmpty()
                                .FirstOrDefaultAsync(s => s.SurgeryID == selectedId);





            if (selectedSurgery == null)
            {
                _logger.LogWarning("Selected surgery is null for SurgeryID: {SurgeryID}", selectedId);
                return NotFound();
            }

            if (selectedSurgery.Patients == null)
            {
                _logger.LogWarning("Patient is null for SurgeryID: {SurgeryID}", selectedSurgery.SurgeryID);
                return NotFound();
            }

            //var surgery_treatmentCode = selectedSurgery.Surgery_TreatmentCodes?.Description;
            var bedSelectList = new SelectList(_context.Bed, "BedId", "BedName");
            ViewBag.Bed = bedSelectList;

            var model = new AdmissionVM
            {
                BedId = 0,
                Date = DateTime.Now,
                Time = DateTime.Now.ToString("tt"),
                //TreatmentCode = treatmentCode,
                SurgeryID = selectedSurgery.SurgeryID,
                PatientID= selectedSurgery.PatientID,
                Name = selectedSurgery.Patients.Name ?? "Unknown",
                Surname = selectedSurgery.Patients.Surname ?? "Unknown",
                //Description = selectedSurgery.Surgery_TreatmentCodes.Description

            };

            ViewBag.Wards = new SelectList(_context.Ward.Select(w => new
            {
                WardId = w.WardId.ToString(),  // Convert to string
                w.WardName
            }).ToList(), "WardId", "WardName");
            ViewBag.Bed = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text"); // Initially empty, will be populated on ward selection
            //ViewBag.TreatmentCodes = new SelectList(_context.Ward.ToList(), "Surgery_TreatmentCodeID", "Description");

            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(AdmissionVM model, int selectedId)
        {
            
            var nurseClaim = User.Claims.FirstOrDefault(c => c.Type == "NurseID");
            if (nurseClaim != null && int.TryParse(nurseClaim.Value, out int nurseID))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        // Validate Ward selection
                        if (model.WardId <= 0)
                        {
                            ModelState.AddModelError("WardId", "Ward selection is required");
                            return View(model);
                        }

                        // Validate Bed selection
                        if (model.BedId <= 0)
                        {
                            ModelState.AddModelError("BedId", "Bed selection is required");
                            return View(model);
                        }

                        var selectedSurgery = await _context.Surgeries
                                                    .Include(s => s.Patients)
                                                    //.Include(s => s.Surgery_TreatmentCodes)
                                                    .FirstOrDefaultAsync(s => s.SurgeryID == selectedId);

                      

                        if (selectedSurgery?.Patients == null)
                        {
                            _logger.LogWarning("Patient information is missing for SurgeryID: {SurgeryID}", selectedId);
                            return NotFound();
                        }

                        var patient = selectedSurgery.Patients;

                        // Get the selected bed
                        var selectedBed = await _context.Bed
                            .Include(b => b.Wards)
                            .FirstOrDefaultAsync(b => b.BedId == model.BedId);

                        if (selectedBed == null)
                        {
                            _logger.LogWarning("Selected bed not found: {BedId}", model.BedId);
                            ModelState.AddModelError("BedId", "Selected bed not found.");
                            return View(model);
                        }

                        // Ensure the selected bed is in the correct ward
                        if (selectedBed.Wards.WardId != model.WardId)
                        {
                            _logger.LogWarning("Selected bed {BedId} is not in the selected ward {WardId}", model.BedId, model.WardId);
                            ModelState.AddModelError("BedId", "Selected bed is not in the selected ward.");
                            return View(model);
                        }

                          // Save BedId to the patient
                           patient.BedId = model.BedId;
                            _context.Update(patient);

                        //patient.TreatmentCodeID = model.Surgery_TreatmentCodeID;
                        //patient.WardId = model.WardId;
                        //selectedSurgery.Patient.BedId = model.BedId;
                        // Convert SelectedCondition to a name if it's an ID

                        string bedName = await GetBedById(model.SelectedBed);
                        string wardName = await GetWardById(model.SelectedWard);

                        var admission = new Admission
                        {
                            Date = model.Date,
                            Time = model.Time,
                            PatientID = selectedSurgery.PatientID,


                        };

                        _context.Admissions.Add(admission);
                        TempData["SuccessMessage"] = "Patient successfully admitted.";
                        await _context.SaveChangesAsync();

                        // Optionally, add additional logic here like inserting specific treatment codes
                        //await InsertSpecificTreatmentCode(model.Surgery_TreatmentCodeID, nurseID);

                        _logger.LogInformation("Patient admitted successfully.");
                        return RedirectToAction("DisplayPatientInfo");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while admitting the patient: {Message}", ex.Message);
                        ModelState.AddModelError("", "Unable to save changes.");
                    }
                }
                else
                {
                    _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                }

                // Reload dropdowns or other necessary data for the view
                ViewBag.Wards = new SelectList(_context.Ward.Select(w => new
                {
                    WardId = w.WardId.ToString(),  // Convert to string
                    w.WardName
                }).ToList(), "WardId", "WardName");
                ViewBag.Beds = new SelectList(_context.Bed.Where(b => b.WardId == model.WardId).ToList(), "BedId", "BedName");
                //ViewBag.TreatmentCodes = new SelectList(_context.Surgery_TreatmentCodes.ToList(), "Surgery_TreatmentCodeID", "Description");
            }
            else
            {
                _logger.LogWarning("Nurse ID claim is missing or invalid.");
                ModelState.AddModelError("", "Unable to identify nurse.");
            }

            return View(model);
        }
        private async Task<int> GetBedIdByName(string selectedBed)
        {
            if (int.TryParse(selectedBed, out int bedId))
            {
                return bedId;
            }
            var bed = await _context.Bed.FirstOrDefaultAsync(b => b.BedName == selectedBed);
            return bed?.BedId ?? 0;
        }

        private async Task<int> GetWardIdByName(string selectedWard)
        {
            if (int.TryParse(selectedWard, out int wardId))
            {
                return wardId;
            }
            var ward = await _context.Ward.FirstOrDefaultAsync(w => w.WardName == selectedWard);
            return ward?.WardId ?? 0;
        }

        public async Task<JsonResult> GetBedsByWard(int wardId)
        {
            var beds = await _context.Bed
                                     .Where(b => b.WardId == wardId)
                                     .Select(b => new
                                     {
                                         BedID = b.BedId,
                                         Name = b.BedName
                                     })
                                     .ToListAsync();

            return Json(beds);
        }
        private async Task<string> GetBedById(string selectedBed)
        {
            if (int.TryParse(selectedBed, out int bedId))
            {
                var bed = await _context.Bed.FindAsync(bedId);
                return bed?.BedName;
            }
            return selectedBed;
        }

        private async Task<string> GetWardById(string selectedWard)
        {
            if (int.TryParse(selectedWard, out int wardId))
            {
                var ward = await _context.Ward.FindAsync(wardId);
                return ward?.WardName;
            }
            return selectedWard;
        }

        [HttpPost]
        public IActionResult Wards()
        {
            ViewBag.Wards = new SelectList(_context.Ward.Select(w => new
            {
                WardId = w.WardId.ToString(),  // Convert to string
                w.WardName
            }).ToList(), "WardId", "WardName");

            return View();
        }
        [HttpGet]
        public JsonResult GetBeds(int wardId)
        {
            var beds = GetBedsForWard(wardId);
            return Json(beds);

        }
        private List<SelectListItem> GetBedsForWard(int wardId)
        {
            var beds = new List<SelectListItem>();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT BedId, BedName FROM Bed WHERE WardId = @WardId", connection))
                {
                    command.Parameters.AddWithValue("@WardId", wardId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            beds.Add(new SelectListItem
                            {
                                Value = reader["BedId"].ToString(),
                                Text = reader["BedName"].ToString()
                            });
                        }
                    }
                }
            }
            return beds;
        }

        //private List<SelectListItem> GetWards()
        //    {
        //        var wards = new List<SelectListItem>();
        //        using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        //        {
        //            connection.Open();
        //            using (var command = new SqlCommand("SELECT WardId, WardName FROM Ward", connection))
        //            {
        //                using (var reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        wards.Add(new SelectListItem
        //                        {
        //                            Value = reader["WardId"].ToString(),
        //                            Text = reader["WardName"].ToString()
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //        return wards;
        //    }


        private async Task InsertSpecificTreatmentCode(int treatmentCodeId, int userId)
        {
            string treatmentCodeTable = treatmentCodeId switch
            {

                6 => "01N50ZZ",
                7 => "01N54ZZ",
                8 => "64721",
                9 => "0CBP0ZX",
                10 => "0CBQ0ZX",
                _ => throw new ArgumentException("Invalid ward ID")
            };

            string query = $"INSERT INTO [{treatmentCodeTable}] (UserID) VALUES (@UserID)";
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
                throw new Exception($"Failed to insert into {treatmentCodeTable} table: {ex.Message}");
            }
        }


        public List<SelectListItem> GetTreatmentCode()
        {
            List<SelectListItem> treatmentCode = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT TreatmentCodeID, ICD_10_Code FROM [TreatmentCode]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                treatmentCode.Add(new SelectListItem
                                {
                                    Value = reader["TreatmentCodeID"].ToString(),
                                    Text = reader["ICD_10_Code"].ToString(),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception($"Failed to fetch treatmentcode: {ex.Message}");
            }

            return treatmentCode;
        }
        public async Task<IActionResult> PatientConditions(string id)
        {
            //ViewBag.Username = HttpContext.Session.GetString("Username");

            var viewModel = new CamVM();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("GetPatientDetailsBySurgeryID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IDNo", id);
                    using (var reader = command.ExecuteReader())
                    {
                        // Read patient details
                        if (reader.Read())
                        {
                            viewModel.Patient = new Patient
                            {
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                Surname = reader.IsDBNull(reader.GetOrdinal("Surname")) ? null : reader.GetString(reader.GetOrdinal("Surname")),
                            };
                        }


                        // Move to next result set (allergies)
                        reader.NextResult();
                        // Read allergies
                        viewModel.Allergies = new List<Allergy>();
                        while (reader.Read())
                        {
                            viewModel.Allergies.Add(new Allergy
                            {
                                Name = reader.GetString(reader.GetOrdinal("AllergyName")),
                            });
                        }

                        // Move to next result set (conditions)
                        reader.NextResult();
                        // Read conditions
                        viewModel.Conditions = new List<Condition>();
                        while (reader.Read())
                        {
                            viewModel.Conditions.Add(new Condition
                            {
                                Name = reader.GetString(reader.GetOrdinal("ConditionName")),
                            });
                        }

                        // Move to next result set (conditions)
                        reader.NextResult();
                        // Read conditions
                        viewModel.General_Medication = new List<General_Medication>();
                        while (reader.Read())
                        {
                            viewModel.General_Medication.Add(new General_Medication
                            {
                                Name = reader.GetString(reader.GetOrdinal("MedicationName")),
                            });
                        }
                    }
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SaveVitals(int id)
        {
            try
            {
                var model = new VitalsVM();

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Get patient information
                    using (var command = new SqlCommand("sp_GetPatientInfo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientID", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.PatientID = id;
                                model.Name = reader["Name"].ToString();
                                model.Surname = reader["Surname"].ToString();
                                //model.Weight = reader["Weight"] != DBNull.Value ? (string?)reader["Weight"] : null;
                                //model.Height = reader["Height"] != DBNull.Value ? (string?)reader["Height"] : null;
                            }
                            else
                            {
                                return NotFound("Patient not found.");
                            }
                        }
                    }

                    // Get vitals list
                    model.Vitals = new List<Patient_VitalsVM>();
                    using (var command = new SqlCommand("sp_GetVitalsList", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                model.Vitals.Add(new Patient_VitalsVM
                                {
                                    Vital = reader["Vital"].ToString(),
                                    Min = (string)reader["Min"],
                                    Max = (string)reader["Max"],
                                    Value = string.Empty,
                                    Height=string.Empty,
                                    Weight=string.Empty,
                                    Notes = string.Empty
                                });
                            }
                        }
                    }
                }

                model.Date = DateTime.Now.Date;
                model.Time = DateTime.Now.TimeOfDay;

                return View(model);
            }
            catch (SqlException ex)
            {
                // Log the exception
                return StatusCode(500, "An error occurred while retrieving patient data.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVitals(VitalsVM model)
        {
            if (!ModelState.IsValid)
            {
                // Handle invalid model state
                return View(model);
            }

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("InsertVitals", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@PatientID", model.PatientID);
                        command.Parameters.AddWithValue("@Date", model.Date);
                        command.Parameters.AddWithValue("@Time", model.Time);
                        command.Parameters.AddWithValue("@Height", model.Height ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Weight", model.Weight ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@VitalsData", JsonConvert.SerializeObject(model.Vitals));
                        command.Parameters.AddWithValue("@Notes", model.Notes);


                        await command.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("DisplayVitalsPerPatient", new { model.PatientID });
            }
            catch (SqlException ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, "An error occurred while saving vitals data.");
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> DisplayVitalsPerPatient(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var viewModel = new DisplayVitalsVM();
            viewModel.PatientID = id; // Add this line to set the PatientID
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("GetPatientVitals", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientID", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read patient details
                        if (await reader.ReadAsync())
                        {
                            viewModel.Patient = new Patient
                            {
                                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                                Surname = reader.IsDBNull(reader.GetOrdinal("Surname")) ? null : reader.GetString(reader.GetOrdinal("Surname")),
                                // Add other patient properties as needed
                            };
                        }
                        // Read patient details
                        if (await reader.ReadAsync())
                        {
                            viewModel.Patient_Vitals = new Patient_Vitals
                            {
                                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetString(reader.GetOrdinal("Weight")),
                                Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : reader.GetString(reader.GetOrdinal("Height")),
                                
                            };
                        }
                        // Move to next result set (vitals)
                        if (await reader.NextResultAsync())
                        {
                            viewModel.Vitals = new List<VitalsViewM>();
                            while (await reader.ReadAsync())
                            {
                                viewModel.Vitals.Add(new VitalsViewM
                                {
                                    Vital = reader.IsDBNull(reader.GetOrdinal("Vital")) ? null : reader.GetString(reader.GetOrdinal("Vital")),
                                    Value = reader.IsDBNull(reader.GetOrdinal("Value")) ? null : reader.GetString(reader.GetOrdinal("Value")),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Time = reader.GetTimeSpan(reader.GetOrdinal("Time")),
                                });
                            }
                        }
                        // Move to next result set (vitals)
                        //if (await reader.NextResultAsync())
                        //{
                        //    viewModel.Patient_Vitals = new List<Patient_Vitals>();
                        //    while (await reader.ReadAsync())
                        //    {
                        //        viewModel.Patient_Vitals.Add(new Patient_Vitals
                        //        {
                        //            Value = reader.IsDBNull(reader.GetOrdinal("Value")) ? null : reader.GetString(reader.GetOrdinal("Value")),
                        //            Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? null : reader.GetString(reader.GetOrdinal("Weight")),
                        //            Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? null : reader.GetString(reader.GetOrdinal("Height")),
                        //            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                        //            Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                        //            Time = reader.GetTimeSpan(reader.GetOrdinal("Time")),
                        //            // Add other vital properties as needed
                        //        });
                        //    }
                        //}
                    }
                }
            }
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Suburbs)
                .ThenInclude(s => s.City)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return NotFound();
            }

            var viewModel = new PatientVM
            {
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                DateOfBirth = patient.DateOfBirth,
                IDNo = patient.IDNo,
                Gender = patient.Gender,
                AddressLine1 = patient.AddressLine1,
                AddressLine2 = patient.AddressLine2,
                Email = patient.Email,
                ContactNo = patient.ContactNo,
                NextOfKinNo = patient.NextOfKinNo,
                SuburbID = patient.SuburbID,
                CityID = patient.Suburbs?.City?.CityID ?? 0
            };

            ViewBag.Cities = new SelectList(await _context.Cities.ToListAsync(), "CityID", "Name");
            ViewBag.Suburbs = new SelectList(await _context.Suburbs.Where(s => s.CityID == viewModel.CityID).ToListAsync(), "SuburbID", "Name");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(PatientVM model)
        {
            if (ModelState.IsValid)
            {
                var patient = await _context.Patients.FindAsync(model.PatientID);

                if (patient == null)
                {
                    return NotFound();
                }

                patient.Name = model.Name;
                patient.Surname = model.Surname;
                patient.DateOfBirth = model.DateOfBirth;
                patient.IDNo = model.IDNo;
                patient.Gender = model.Gender;
                patient.AddressLine1 = model.AddressLine1;
                patient.AddressLine2 = model.AddressLine2;
                patient.Email = model.Email;
                patient.ContactNo = model.ContactNo;
                patient.NextOfKinNo = model.NextOfKinNo;
                patient.SuburbID = model.SuburbID;

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index)); // or wherever you want to redirect after successful edit
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(model.PatientID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var cities = _context.Cities.Select(c => new SelectListItem
            {
                Value = c.CityID.ToString(),
                Text = c.Name
            }).ToList();

            cities.Insert(0, new SelectListItem { Value = "", Text = "Select City" });

            // Populate ViewBag.Cities
            ViewBag.Cities = cities;
            ViewBag.Cities = new SelectList(_context.Cities.Select(c => new
            {
                CityID = c.CityID.ToString(),  // Convert to string
                c.Name
            }).ToList(), "CityID", "Name");

            //ViewBag.Cities = new SelectList(await _context.Cities.ToListAsync(), "CityID", "Name");
            ViewBag.Suburbs = new SelectList(await _context.Suburbs.Where(s => s.CityID == model.CityID).ToListAsync(), "SuburbID", "Name");

            return View(model);
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientID == id);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetSuburbsByCity(int cityId)
        //{
        //    var suburbs = await _context.Suburbs
        //        .Where(s => s.CityID == cityId)
        //        .Select(s => new { value = s.SuburbID, text = s.Name })
        //        .ToListAsync();

        //    return Json(suburbs);
        //}
        [HttpGet]
        public async Task<IActionResult> UpdatePatientProfile(int id)
        {
            //var UserID = _userService.GetLoggedInUserId();

            var pateint = await _context.Patients
        .Include(p => p.Suburbs)
        .ThenInclude(s => s.City)
        .ThenInclude(c => c.Province)
        .FirstOrDefaultAsync(p => p.PatientID == id);

            var updatePatient = new PatientVM
            {
                PatientID = pateint.PatientID,
                Name = pateint.Name,
                Gender = pateint.Gender,
                Surname = pateint.Surname,
                Email = pateint.Email,
                ContactNo = pateint.ContactNo,
                IDNo = pateint.IDNo,
                DateOfBirth = pateint.DateOfBirth,
                NextOfKinNo = pateint.NextOfKinNo,
                AddressLine1 = pateint.AddressLine1,
                AddressLine2 = pateint.AddressLine2,

            };
            ViewBag.Provinces = await GetProvincesAsync();
            ViewBag.Cities = await GetCitiesByProvinceAsync(pateint.Suburbs.City.ProvinceID);
            ViewBag.Suburbs = await GetSuburbsByCityAsync(pateint.Suburbs.CityID);



            return View(updatePatient);
        }

        public async Task<IActionResult> SubmitPatientProfile(PatientVM model, int provinceId)
        {
            if (ModelState.IsValid)
            {
                var pateint = await _context.Patients
           .Include(p => p.Suburbs)
           .ThenInclude(s => s.City)
           .FirstOrDefaultAsync(p => p.PatientID == model.PatientID);

                if (pateint == null)
                {
                    return NotFound();
                }


                pateint.Name = model.Name;
                pateint.Surname = model.Surname;
                pateint.Gender = model.Gender;
                pateint.Email = model.Email;
                pateint.ContactNo = model.ContactNo;
                pateint.IDNo = model.IDNo;
                pateint.AddressLine1 = model.AddressLine1;
                pateint.AddressLine2 = model.AddressLine2;
                pateint.DateOfBirth = model.DateOfBirth;
                pateint.NextOfKinNo = model.NextOfKinNo;
                pateint.SuburbID = model.SuburbID;
                //pateint.Suburbs.CityID = model.CityID;
                //pateint.Suburbs.City.ProvinceID = model.ProvinceID;

                // Update related entities if necessary
                if (pateint.Suburbs != null)
                {
                    pateint.Suburbs.CityID = model.CityID;
                    if (pateint.Suburbs.City != null)
                    {
                        pateint.Suburbs.City.ProvinceID = model.ProvinceID;
                    }
                }

                _context.Patients.Update(pateint);
                await _context.SaveChangesAsync();
                return RedirectToAction("PatientProfile", "Profile");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Provinces = await GetProvincesAsync();
            ViewBag.Cities = await GetCitiesByProvinceAsync(model.ProvinceID);
            ViewBag.Suburbs = await GetSuburbsByCityAsync(model.CityID);

            return View(model);
        }
        public async Task<ActionResult> PatientsAd()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");



            var patientAd = _context.Surgeries
                             .Include(s => s.Patients)
                              .ThenInclude(s => s.Beds)
                             //.Include(s => s.Theatres)
                             //.Include(s => s.Anaesthesiologists)
                             //.Include(s => s.Surgery_TreatmentCodes)
                             .Select(s => new AdmissionVM
                             {
                                 SurgeryID = s.SurgeryID,
                                 PatientID = s.PatientID,
                                 //AnaesthesiologistID = s.AnaesthesiologistID,
                                 //TheatreID = s.TheatreID,
                                 BedName=s.Patients.Beds.BedName,
                                 WardName=s.Patients.Beds.Wards.WardName,
                                 //Surgery_TreatmentCodeID = s.Surgery_TreatmentCodeID,
                                 //ICD_Code_10 = s.Surgery_TreatmentCodes.ICD_10_Code,
                                 Name = s.Patients.Name,
                                 Surname = s.Patients.Surname,
                                 //TheatreName = s.Theatres.Name,
                                 //AnaesthesiologistName = s.Anaesthesiologists.User.Name,
                                 //AnaesthesiologistSurname = s.Anaesthesiologists.User.Surname,
                                 Date = s.Date,
                                 Time = s.Time // Assuming these properties exist on your Surgery entity
                             })
                              .ToList();


            return View(patientAd);
        }
        [HttpGet]
        public async Task<ActionResult> SurgeriesBooked(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var surgeries = await _context.SurgeryDetailsViewModel.FromSqlRaw(
                "EXEC GetBookedSurgeryDetails"
            ).ToListAsync();

            // Apply date filtering in memory
            if (startDate.HasValue)
            {
                surgeries = surgeries.Where(s => s.Date >= startDate.Value.Date).ToList();
            }
            if (endDate.HasValue)
            {
                surgeries = surgeries.Where(s => s.Date <= endDate.Value.Date).ToList();
            }

            // Convert the comma-separated string of surgery codes to a list
            //foreach (var surgery in surgeries)
            //{
            //    surgery.SurgeryCodes = surgery.SurgeryCode?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            //}

            // Pass the date values to the view for maintaining filter state
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(surgeries);
        }
        [HttpGet]
        public async Task<ActionResult>GetBookedSurgeries(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Surgeries
                .Include(s => s.Patients)
                .Include(s => s.Anaesthesiologists).ThenInclude(a => a.User)
                  .Include(s => s.Surgeons).ThenInclude(a => a.User)
                .Include(s => s.Theatres)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.Date <= endDate.Value);

            var bookedSurgeries = await query
                .Select(s => new BookedSurgeriesVM
                {
                    SurgeryID = s.SurgeryID,
                    Date = s.Date,
                    Time = s.Time,
                    PatientName = s.Patients.Name,
                    PatientSurname = s.Patients.Surname,
                    AnaesthesiologistName = s.Anaesthesiologists.User.Name,
                    AnaesthesiologistSurname = s.Anaesthesiologists.User.Surname,
                    SurgeonName=s.Surgeons.User.Name,
                    SurgeonSurname=s.Surgeons.User.Surname,
                    TheatreName = s.Theatres.Name
                    
                })
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            return View(bookedSurgeries);
        }
    }
}

        
       













     


