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

        [HttpGet]
        public async Task<ActionResult> DisplayPatientInfo(int selectedId)
        {
            // Await the first asynchronous operation to ensure it completes before proceeding
            //var selectedSurgery = await _context.Admissions
            //                                    .Include(s => s.Patient)
            //                                    .FirstOrDefaultAsync(s => s.AdmissionID == selectedId);

            // Proceed with the second asynchronous operation
            var patient = await _context.Patients
                                        .Include(p => p.Suburbs)
                                        .ThenInclude(suburb => suburb.City)
                                        .Where(p => p.PatientID == selectedId)
                                        .Select(p => new PatientVM
                                        {
                                            PatientID = p.PatientID,
                                            Name = p.Name,
                                            Surname = p.Surname,
                                            DateOfBirth = p.DateOfBirth,
                                            IDNo = p.IDNo,
                                            Gender = p.Gender,
                                            AddressLine1 = p.AddressLine1,
                                            AddressLine2 = p.AddressLine2,
                                            Email = p.Email,
                                            ContactNo = p.ContactNo,
                                            NextOfKinNo = p.NextOfKinNo,
                                            Status = p.Status,
                                            TreatmentCodeID = p.TreatmentCodeID,
                                            SuburbID = p.SuburbID,
                                            SuburbName = p.Suburbs.Name,
                                            PostalCode = p.Suburbs.PostalCode,
                                            CityName = p.Suburbs.City.Name,
                                            CityID = p.Suburbs.City.CityID
                                        })
                                        .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            // Load cities for dropdown
            var cities = await _context.Cities.ToListAsync();
            ViewBag.Cities = new SelectList(cities, "CityID", "Name");

            // Load suburbs for dropdown based on the current patient's city
            var suburbs = await _context.Suburbs
                                         .Where(s => s.CityID == patient.CityID)
                                         .ToListAsync();
            ViewBag.Suburbs = new SelectList(suburbs, "SuburbID", "Name");

            return View(patient);
        }
        //[HttpGet]
        //public async Task<ActionResult> GetSuburbsByCity(int cityId)
        //{
        //    var suburbs = await _context.Suburbs
        //                                 .Where(s => s.CityID == cityId)
        //                                 .ToListAsync();

        //    return Json(suburbs.Select(s => new
        //    {
        //        s.SuburbID,
        //        s.Name
        //    }));
        //}

        [HttpPost]
        public async Task<IActionResult> DisplayPatientInfo(PatientVM model)
        {
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogWarning("Model state is not valid.");
            //    return View(model);
            //}


            //// Retrieve the existing patient record
            //var patient = await _context.Patients
            //                                .Include(p => p.Suburbs)
            //                                .ThenInclude(suburb => suburb.City)
            //                                .FirstOrDefaultAsync(p => p.PatientID == model.PatientID);

            //if (patient == null)
            //{
            //    return NotFound();
            //}

            //// Update patient properties based on the submitted model
            //patient.Name = model.Name;
            //patient.Surname = model.Surname;
            //patient.DateOfBirth = model.DateOfBirth;
            //patient.IDNo = model.IDNo;
            //patient.Gender = model.Gender;
            //patient.AddressLine1 = model.AddressLine1;
            //patient.AddressLine2 = model.AddressLine2;
            //patient.Email = model.Email;
            //patient.ContactNo = model.ContactNo;
            //patient.NextOfKinNo = model.NextOfKinNo;
            ////patient.Status = model.Status;
            ////patient.TreatmentCodeID = model.TTreatmentCodeID;
            //patient.SuburbID = model.SuburbID;

            //// Update Suburb and City based on the selected suburb ID
            //var suburb = await _context.Suburbs
            //                           .Include(s => s.City)
            //                           .FirstOrDefaultAsync(s => s.SuburbID == model.SuburbID);

            //if (suburb != null)
            //{
            //    patient.Suburbs = suburb;
            //}

            //// Save changes to the database
            //await _context.SaveChangesAsync();

            return RedirectToAction("DisplayVitals", "Nurse");
            //return RedirectToAction(nameof(DisplayPatientInfo), new { selectedId = patient.PatientID });
        }
    

            // If model state is invalid, reload the dropdowns and return the view with the model
            //var cities = await _context.Cities.ToListAsync();
            //ViewBag.Cities = new SelectList(cities, "CityID", "Name");

            //var suburbs = await _context.Suburbs
            //                            .Where(s => s.CityID == model.CityID)
            //                            .ToListAsync();
            //ViewBag.Suburbs = new SelectList(suburbs, "SuburbID", "Name");

            //return View(model);


        
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
                SurgeryID = selectedAdmission.SurgeryID,
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
                SurgeryID = selectedSurgery.SurgeryID,
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
        public async Task<ActionResult> PatientsVitals(string searchString)
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
                                 Date = s.Date,
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


            var patientAdmission = from a in _context.Admissions
                                   join p in _context.Patients on a.PatientID equals p.PatientID
                                   join b in _context.Bed on p.BedId equals b.BedId
                                   join t in _context.TreatmentCodes on p.TreatmentCodeID equals t.TreatmentCodeID
                                   join n in _context.Anaesthesiologists on a.AnaesthesiologistID equals n.AnaesthesiologistID
                                   join s in _context.Surgeons on a.SurgeonID equals s.SurgeonID
                                   join u_surgeon in _context.Users on s.UserID equals u_surgeon.UserID
                                   join u_anaesth in _context.Users on n.UserID equals u_anaesth.UserID
                                   select new AdmittedPatientsVM
                                   {
                                       AdmissionID = a.AdmissionID,
                                       Date = a.Date,
                                       Time = a.Time,
                                       Patients = p.Name + " " + p.Surname,
                                       Gender = p.Gender,
                                       Bed = b.BedName,
                                       TreatmentCodes = t.Description,
                                       Anaesthesiologists = u_anaesth.Name + " " + u_anaesth.Surname,
                                       Surgeons = u_surgeon.Name + " " + u_surgeon.Surname
                                   };

            if (!string.IsNullOrEmpty(searchString))
            {
                patientAdmission = patientAdmission.Where(pa => pa.Patients.Contains(searchString));
            }

            var result = await patientAdmission.ToListAsync();
            return View(result);
        }
        public IActionResult Prescription()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public IActionResult Discharge()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
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
        public async Task<ActionResult> Codition2(string id)
        {
            var viewModel = new ConditionsVM
            {
                IDNo = id,
                Condition = await GetConditionsAsync(),
                Allergy = await GetAllergiesAsync(),
                General_Medication = await GetMedicationsAsync()
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Codition2(ConditionsVM model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            if (!ModelState.IsValid)
            {
                // If the model is invalid, reload the dropdown data and return to the view
                model.Condition = await GetConditionsAsync();
                model.Allergy = await GetAllergiesAsync();
                model.General_Medication = await GetMedicationsAsync();
                return View(model);
            }

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("AddPatientDetails", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@IDNo", model.IDNo);
                        command.Parameters.AddWithValue("@ConditionName", model.SelectedCondition);
                        command.Parameters.AddWithValue("@AllergyName", model.SelectedAllergy);
                        command.Parameters.AddWithValue("@MedicationName", model.SelectedMedication);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("PatientConditions", new { id = model.IDNo });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Reload dropdown data
                model.Condition = await GetConditionsAsync();
                model.Allergy = await GetAllergiesAsync();
                model.General_Medication = await GetMedicationsAsync();

                ModelState.AddModelError("", "An error occurred while saving the data. Please try again.");
                return View(model);
            }
        }




        private async Task<List<SelectListItem>> GetConditionsAsync()
        {
            // Fetch conditions from database and return as SelectListItems
            // This is a placeholder implementation
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "9", Text = "Attention Deficit Disorder" },
            new SelectListItem { Value = "10", Text = "Hyperthyroidism" },
             new SelectListItem { Value = "11", Text = "Migraine" },
            new SelectListItem { Value = "12", Text = "Back Pain" },
             new SelectListItem { Value = "13", Text = "Hypertension" },
            new SelectListItem { Value = "14", Text = "Asthma " },
             new SelectListItem { Value = "15", Text = "High Cholesterol" },
           
            // Add more conditions...
        };
        }

        private async Task<List<SelectListItem>> GetAllergiesAsync()
        {
            // Fetch allergies from database and return as SelectListItems
            // This is a placeholder implementation
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Food Allergies" },
            new SelectListItem { Value = "2", Text = "Seasonal Allergies" },
             new SelectListItem { Value = "3", Text = "Mold Allergies" },
            // Add more allergies...
        };
        }

        private async Task<List<SelectListItem>> GetMedicationsAsync()
        {
            // Fetch medications from database and return as SelectListItems
            // This is a placeholder implementation
            return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Ibupain" },
            //new SelectListItem { Value = "Medication2", Text = "Medication 2" },
            // Add more medications...
        };
        }

        [HttpGet]
        public async Task<ActionResult> AddCodition2(int selectedId)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var selectedAdmission = await _context.Surgeries
                .Include(a => a.Patients)
                .FirstOrDefaultAsync(a => a.SurgeryID == selectedId);

            if (selectedAdmission == null)
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
                 .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);

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
                                 Date = s.Date,
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
        public IActionResult RetakeVitals()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> RetakeVitals2(int selectedId)
        {
            //if (selectedId <= 0)
            //{
            //    _logger.LogWarning("Invalid admission id: {Id}", selectedId);
            //    return NotFound("Invalid admission ID.");
            //}

            //var selectedAdmission = await _context.Surgeries
            //    .Include(a => a.Patients)
            //    .FirstOrDefaultAsync(a => a.SurgeryID == selectedId);

            //if (selectedAdmission == null)
            //{
            //    _logger.LogWarning("Admission not found with id: {Id}", selectedId);
            //    return NotFound("Admission not found.");
            //}

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedId);

            //if (patient == null)
            //{
            //    _logger.LogWarning("Patient not found with id: {Id}", model.PatientID);
            //    return NotFound("Patient not found.");
            //}

            var vitalsList = await _context.Vitals
       .Select(v => new Patient_VitalsVM
       {
           Vital = v.Vital,
           Min = v.Min,
           Max = v.Max,
           Notes = string.Empty
       }).ToListAsync();

            //Initialize the view model with the necessary data
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
        public async Task<IActionResult> RetakeVitals2(VitalsVM model)
        {

            if (!ModelState.IsValid)
            {
                //foreach (var state in ModelState)
                //{
                //    foreach (var error in state.Value.Errors)
                //    {
                //        _logger.LogWarning("ModelState Error: {Key} - {ErrorMessage}", state.Key, error.ErrorMessage);
                //    }
                //}

                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                //// Existing logic to repopulate the model and return the view
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == model.PatientID);
                if (patient != null)
                {
                    model.Name = patient.Name;
                    model.Surname = patient.Surname;
                }
                // Re-populate Vitals list with normal ranges
                var vitalsList = new List<Patient_VitalsVM>();
                foreach (var v in await _context.Vitals.ToListAsync())
                {
                    var vitalVM = model.Vitals.FirstOrDefault(vm => vm.Vital == v.Vital);

                    vitalsList.Add(new Patient_VitalsVM
                    {
                        Vital = v.Vital,
                        Min = v.Min,
                        Max = v.Max,
                        Value = vitalVM?.Value ?? string.Empty,
                        Notes = vitalVM?.Notes ?? string.Empty
                    });
                }
                model.Vitals = vitalsList;
                return View(model);
            }
            // Debug and inspect model.Vitals
            foreach (var vital in model.Vitals)
            {
                System.Diagnostics.Debug.WriteLine($"Vital: {vital.Vital}, Value: {vital.Value}");
            }

            try
            {
                // Verify that the PatientID exists in the Patients table
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientID == model.PatientID);
                if (!patientExists)
                {
                    _logger.LogWarning("Patient with ID: {PatientID} does not exist.", model.PatientID);
                    return NotFound("Patient not found.");
                }

                // List to hold all vitals being added
                var vitalsList = new List<Patient_Vitals>();

                foreach (var vitalVM in model.Vitals)
                {
                    var vital = new Patient_Vitals
                    {
                        PatientID = model.PatientID,
                        VitalsID = _context.Vitals
                            .Where(v => v.Vital == vitalVM.Vital)
                            .Select(v => v.VitalsID)
                            .FirstOrDefault(), // Assuming Vital is unique
                        Date = model.Date,
                        Time = model.Time,
                        Value = vitalVM.Value,
                        Notes = vitalVM.Notes,
                        //Weight = model.Weight,
                        //Height = model.Height
                    };

                    // Log the vital information being added
                    _logger.LogInformation($"Adding vital for PatientID: {model.PatientID}, VitalID: {vital.VitalsID}, Value: {vital.Value}");

                    vitalsList.Add(vital);
                }
                // Check if any vitals were added before saving
                if (vitalsList.Any())
                {
                    _context.Patient_Vitals.AddRange(vitalsList);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Vitals data saved successfully.");
                }
                else
                {
                    _logger.LogWarning("No valid vitals to save for PatientID: {PatientID}.", model.PatientID);
                    ModelState.AddModelError(string.Empty, "No valid vitals to save.");
                    return View(model);
                }


                return RedirectToAction("DisplayVitals", "Nurse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving vitals data.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving vitals data.");

                // Ensure patient data is included in the model returned to the view
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientID == model.PatientID);
                if (patient != null)
                {
                    model.Name = patient.Name;
                    model.Surname = patient.Surname;
                }

                // Re-populate Vitals list with normal ranges
                var vitalsList = new List<Patient_VitalsVM>();
                foreach (var v in await _context.Vitals.ToListAsync())
                {
                    var vitalVM = model.Vitals.FirstOrDefault(vm => vm.Vital == v.Vital);

                    vitalsList.Add(new Patient_VitalsVM
                    {
                        Vital = v.Vital,
                        Min = v.Min,
                        Max = v.Max,
                        Value = vitalVM?.Value ?? string.Empty, // Handling null values
                        Notes = vitalVM?.Notes ?? string.Empty,
                    });
                }

                model.Vitals = vitalsList;
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
        public IActionResult AdministeredMeds()
        {
            return View();
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
            //int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);
            //int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);
            var nurseClaim = User.Claims.FirstOrDefault(c => c.Type == "NurseID");
            if (nurseClaim != null && int.TryParse(nurseClaim.Value, out int nurseID))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var selectedSurgery = await _context.Surgeries
                                                    .Include(s => s.Patients)
                                                    //.Include(s => s.Surgery_TreatmentCodes)
                                                    .FirstOrDefaultAsync(s => s.SurgeryID == selectedId);

                        var patient = selectedSurgery.Patients;

                        if (selectedSurgery?.Patients == null)
                        {
                            _logger.LogWarning("Patient information is missing for SurgeryID: {SurgeryID}", selectedId);
                            return NotFound();
                        }

                        patient.BedId = model.BedId;


                        //patient.TreatmentCodeID = model.Surgery_TreatmentCodeID;
                        //patient.WardId = model.WardId;
                        //selectedSurgery.Patient.BedId = model.BedId;


                        var admission = new Admission
                        {
                            Date = model.Date,
                            Time = model.Time,
                            PatientID = selectedSurgery.PatientID,

                        };

                        _context.Admissions.Add(admission);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVitals(VitalsVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var vital in model.Vitals)
                    {
                        var parameters = new[]
                        {
                        new SqlParameter("@PatientID", model.PatientID),
                        new SqlParameter("@VitalsID", vital.VitalsID),
                        new SqlParameter("@Date", model.Date),
                        new SqlParameter("@Time", model.Time),
                        new SqlParameter("@Value", vital.Value),
                        new SqlParameter("@Notes", model.Notes ?? string.Empty),
                        new SqlParameter("@Height", model.Height ?? string.Empty),
                        new SqlParameter("@Weight", model.Weight ?? string.Empty)
                    };

                        _context.Database.ExecuteSqlRaw("EXEC sp_InsertPatientVitals @PatientID, @VitalsID, @Date, @Time, @Value, @Notes, @Height, @Weight", parameters);
                    }

                    return RedirectToAction("SuccessPage");  // Redirect to a success page after saving
                }
                catch (Exception ex)
                {
                    // Log the exception (ex) and handle it as necessary
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the vitals. Please try again.");
                }
            }

            // Reinitialize any necessary view data and return to the same view in case of failure
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DisplayVitalsPerPatient(string id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var viewModel = new PatientRecordViewModel();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("GetPatientVitals", connection))
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

                        // Move to next result set (vitals)
                        if (reader.NextResult())
                        {
                            viewModel.Vitals = new List<Vitals>();
                            while (reader.Read())
                            {
                                viewModel.Vitals.Add(new Vitals
                                {
                                    Vital = reader.IsDBNull(reader.GetOrdinal("Vital")) ? null : reader.GetString(reader.GetOrdinal("Vital")),
                                    Min = reader.IsDBNull(reader.GetOrdinal("Min")) ? null : reader.GetString(reader.GetOrdinal("Min")),
                                    Max = reader.IsDBNull(reader.GetOrdinal("Max")) ? null : reader.GetString(reader.GetOrdinal("Max")),

                                });
                            }
                        }
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
                SuburbID = patient.SuburbID ?? 0,
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

        [HttpGet]
        public async Task<IActionResult> GetSuburbsByCity(int cityId)
        {
            var suburbs = await _context.Suburbs
                .Where(s => s.CityID == cityId)
                .Select(s => new { value = s.SuburbID, text = s.Name })
                .ToListAsync();

            return Json(suburbs);
        }
    }

}











     


