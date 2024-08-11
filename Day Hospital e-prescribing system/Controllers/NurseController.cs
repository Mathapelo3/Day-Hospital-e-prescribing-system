using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
            return View();
        }
        public IActionResult UpdatePatientInfo()
        {
            return View();
        }
        public IActionResult DisplayPatientInfo()
        {
            return View();
        }
        // GET: Vitals/Add
        public async Task<IActionResult> Vitals(int selectedId)
        {


            if (selectedId <= 0)
            {
                _logger.LogWarning("Invalid admission id: {Id}", selectedId);
                return NotFound("Invalid admission ID.");
            }

            var selectedAdmission = await _context.Admissions
                .Include(a => a.Patients)
                .FirstOrDefaultAsync(a => a.AdmissionID == selectedId);

            if (selectedAdmission == null)
            {
                _logger.LogWarning("Admission not found with id: {Id}", selectedId);
                return NotFound("Admission not found.");
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", selectedAdmission.PatientID);
                return NotFound("Patient not found.");
            }

            var model = new VitalsVM
            {
                AdmissionID = selectedAdmission.AdmissionID,
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = DateTime.Now, // Default to current date for new vitals entry
                Time = TimeSpan.Zero, // Default to current time for new vitals entry
                Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight,
                Height = patient.Patient_Vitals.FirstOrDefault()?.Height,
                Vitals = patient.Patient_Vitals.Select(pv => new Patient_VitalsVM
                {
                    Vital = pv.Vitals.Vital,
                    Value = pv.Value,
                    Date = pv.Date,
                    Time = pv.Time.ToString(@"hh\:mm"),
                    Notes = pv.Notes
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Vitals(VitalsVM model)
        {
            if (ModelState.IsValid)
            {
                var selectedAdmission = await _context.Admissions
                    .Include(a => a.Patients)
                    .FirstOrDefaultAsync(a => a.AdmissionID == model.AdmissionID);

                if (selectedAdmission == null)
                {
                    return NotFound("Admission not found.");
                }

                var patient = await _context.Patients
                    .Include(p => p.Patient_Vitals)
                    .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);

                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                // Update existing or add new vitals entries
                foreach (var vitalsVM in model.Vitals)
                {
                    var existingVitals = patient.Patient_Vitals
                        .FirstOrDefault(pv => pv.Vitals.Vital == vitalsVM.Vital);

                    //if (existingVitals != null)
                    //{
                    //    // Update existing vitals
                    //    existingVitals.Value = vitalsVM.Value;
                    //    existingVitals.Date = model.Date;
                    //    existingVitals.Time = model.Time;
                    //    existingVitals.Notes = vitalsVM.Notes;
                    //}
                    //else
                    
                        // Add new vitals
                        var newVitals = new Patient_Vitals
                        {
                            Date = model.Date,
                            Time = model.Time,
                            Value = vitalsVM.Value,
                            //VitalsID = await GetVitalsId(vitalsVM.Vital),
                            PatientID = model.PatientID,
                            Notes = vitalsVM.Notes,
                        };
                        _context.Patient_Vitals.Add(newVitals);
                    
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Vitals updated successfully for patient id: {Id}", model.PatientID);
                return RedirectToAction("VitalsSuccess");
            }

            return View(model);
        }

        //private async Task<int> GetVitalsId(string vitalName)
        //{
        //    var vital = await _context.Vitals.FirstOrDefaultAsync(v => v.Vital == vitalName);
        //    if (vital == null)
        //    {
        //        throw new Exception($"Vital '{vitalName}' not found in the database.");
        //    }
        //    return vital.VitalsID;
        //}
        public async Task<ActionResult> DisplayVitals(int selectedId)
        {
            var selectedAdmission = await _context.Admissions
               .Include(a => a.Patients)
               .FirstOrDefaultAsync(a => a.AdmissionID == selectedId);

            if (selectedAdmission == null)
            {
                return NotFound("Admission not found.");
            }

        

            if (selectedId <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", selectedId);
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", selectedId);
                return NotFound();
            }

            var model = new VitalsVM
            {
                AdmissionID = selectedAdmission.AdmissionID,
                Name = patient.Name,
                Surname = patient.Surname,
                Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight,
                Height = patient.Patient_Vitals.FirstOrDefault()?.Height,
                Date = patient.Patient_Vitals.FirstOrDefault()?.Date ?? DateTime.Now,
                Time = patient.Patient_Vitals.FirstOrDefault()?.Time ?? TimeSpan.Zero,
                Vitals = patient.Patient_Vitals.Select(pv => new Patient_VitalsVM
                {
                    Vital = pv.Vitals.Vital,
                    Value = pv.Value,
                    //Min = pv.Vitals.Min,
                    //Max = pv.Vitals.Max,

                    Date = pv.Date,
                    Time = pv.Time.ToString(@"hh\:mm"),
                    Notes = pv.Notes
                }).ToList(),
               

            };

            _logger.LogInformation("MedicalHistory action completed successfully for id: {Id}", selectedId);
            return View(model);
        }
        public IActionResult VitalsEdit()
        {
            return View();
        }
        public IActionResult Prescription()
        {
            return View();
        }
        public IActionResult Discharge()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Codition2()
        {

            var allergy = GetAllergies();
            var condition = GetConditions();
            var meds = GetGeneral_Medication();

            var model = new ConditionsVM
            {
                Allergy = allergy,
                Condition = condition,
                General_Medication = meds
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Codition2(ConditionsVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Allergy = GetAllergies();
                model.Condition = GetConditions();
                model.General_Medication = GetGeneral_Medication();
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return View(model);
            }


            int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);

           

            string insertAllergyQuery = "INSERT INTO Patient_Allergy (PatientID, AllergyID) VALUES (@PatientID, @AllergyID)";
            string insertConditionQuery = "INSERT INTO Patient_Condition (PatientID, ConditionID) VALUES (@PatientID, @ConditionID)";
            string insertMedicationQuery = "INSERT INTO Patient_Medication (PatientID, General_MedicationID) VALUES (@PatientID, @General_MedicationID)";



            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Assuming model.PatientID contains the patient ID you want to associate with allergies, conditions, and medications
                    int patientId = model.PatientID;

                    using (SqlCommand allergyCommand = new SqlCommand(insertAllergyQuery, connection))
                    {
                        allergyCommand.Parameters.AddWithValue("@PatientID", patientId);
                        allergyCommand.Parameters.AddWithValue("@AllergyID", model.AllergyID);
                        await allergyCommand.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand conditionCommand = new SqlCommand(insertConditionQuery, connection))
                    {
                        conditionCommand.Parameters.AddWithValue("@PatientID", patientId);
                        conditionCommand.Parameters.AddWithValue("@ConditionID", model.ConditionID);
                        await conditionCommand.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand medicationCommand = new SqlCommand(insertMedicationQuery, connection))
                    {
                        medicationCommand.Parameters.AddWithValue("@PatientID", patientId);
                        medicationCommand.Parameters.AddWithValue("@General_MedicationID", model.General_MedicationID);
                        await medicationCommand.ExecuteNonQueryAsync();
                    }

                    // Redirect or return success message
                    return RedirectToAction("AddCondition2");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the user: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }




        private async Task InsertSpecificAllergy(int allergyId, int userId)
        {
            string allergyTable = allergyId switch
            {
                1 => "Food Allergies",
                2 => "Seasonal Allergies",
                3 => "Mold Allergies",


                _ => throw new ArgumentException("Invalid ward ID")
            };

            string query = $"INSERT INTO [{allergyTable}] (UserID) VALUES (@UserID)";
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
                throw new Exception($"Failed to insert into {allergyTable} table: {ex.Message}");
            }
        }
        private async Task InsertSpecificCondition(int conditionId, int userId)
        {


            string conditionTable = conditionId switch
            {
                4 => "Diabetes",
                5 => "Hypertension",
                6 => "Asthma",
                7 => "Arthritis",
                8 => "Chronic Migraine",
                //5 => "Anaesthesiologist",
                _ => throw new ArgumentException("Invalid ward ID")
            };

            string query = $"INSERT INTO [{conditionTable}] (UserID) VALUES (@UserID)";
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
                throw new Exception($"Failed to insert into {conditionTable} table: {ex.Message}");
            }
        }
        private async Task InsertSpecificGeneral_Medication(int generalMedicationId, int userId)
        {
            string generalMedicationTable = generalMedicationId switch
            {

                1 => "Ibupain",
                //3 => "Fwo5412",
                //4 => "Bsa3164",
                _ => throw new ArgumentException("Invalid ward ID")
            };

            string query = $"INSERT INTO [{generalMedicationTable}] (UserID) VALUES (@UserID)";
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
                throw new Exception($"Failed to insert into {generalMedicationTable} table: {ex.Message}");
            }
        }

        public List<SelectListItem> GetAllergies()
        {
            List<SelectListItem> allergy = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT AllergyID, Name FROM [Allergy]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allergy.Add(new SelectListItem
                                {
                                    Value = reader["AllergyID"].ToString(),
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

            return allergy;
        }
        public List<SelectListItem> GetConditions()
        {
            List<SelectListItem> condition = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT ConditionID, Name FROM [Condition]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        //command.Parameters.AddWithValue("@WardID", wardId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                condition.Add(new SelectListItem
                                {
                                    Value = reader["ConditionID"].ToString(),
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

            return condition;
        }
        public List<SelectListItem> GetGeneral_Medication()
        {
            List<SelectListItem> meds = new List<SelectListItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();
                    string sql = "SELECT General_MedicationID, Name FROM [General_Medication]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                meds.Add(new SelectListItem
                                {
                                    Value = reader["General_MedicationID"].ToString(),
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

            return meds;
        }


        public async Task<ActionResult> AddCodition2(int selectedId, string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            // Get the Admission record based on the AdmissionID
            var selectedAdmission = await _context.Admissions
                .Include(a => a.Patients) // Assuming there is a navigation property for Patient in Admission
                .FirstOrDefaultAsync(a => a.AdmissionID == selectedId);

            if (selectedAdmission == null)
            {
                return NotFound("Admission not found.");
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
          
            var viewModel = new ConditionsVM
            {
                AdmissionID = selectedAdmission.AdmissionID,
                Name = patient.Name,
                Surname = patient.Surname,
                Allergies = patient.Patient_Allergy.Select(pa => pa.Allergy).ToList(),
                Conditions = patient.Patient_Condition.Select(pc => pc.Condition).ToList(),
                Medications = patient.Patient_Medication.Select(pm => pm.General_Medication).ToList(),
               
            };
            return View(viewModel);


        }
        public IActionResult DisplayAdmission()
        {
            return View();
        }
        public async Task<ActionResult> AdmissionWizard(DateTime? Date, string searchString)
        {

            ViewData["CurrentFilter"] = searchString;

            var searchpatient = from s in _context.Patients
                                select s;
            //if (String.IsNullOrEmpty(searchString))
            //{
            //    searchpatient = searchpatient.Where(s => s.Name.Contains(searchString)
            //                           || s.Surname.Contains(searchString));
            //}
            ViewData["CurrentDate"] = Date?.ToString("dd-MM-yyyy");


            var bookedPatients = from s in _context.Surgeries
                                 join p in _context.Patients on s.PatientID equals p.PatientID
                                 join n in _context.Anaesthesiologists on s.AnaesthesiologistID equals n.AnaesthesiologistID
                                 join su in _context.Surgeons on s.SurgeonID equals su.SurgeonID
                                 join t in _context.Theatres on s.TheatreID equals t.TheatreID
                                 join c in _context.Surgery_TreatmentCodes on s.Surgery_TreatmentCodeID equals c.Surgery_TreatmentCodeID
                                 join u in _context.Users on n.UserID equals u.UserID
                                 join us in _context.Users on su.UserID equals us.UserID
                                 select new BookedVM
                                 {
                                     SurgeryID = s.SurgeryID,
                                     PatientDetails = $"{p.Name} {p.Surname}", // Concatenate name and surname for display
                                     Date = s.Date,
                                     Time = s.Time,
                                     Anaesthesiologist = $"{u.Name} {u.Surname}",
                                     Theatre = t.Name,
                                     Surgery_TreatmentCode = c.Description,
                                     Surgeon = $"{u.Name} {u.Surname}"
                                 };

            if (Date.HasValue)
            {
                bookedPatients = bookedPatients.Where(pa => pa.Date.Date == Date.Value.Date);
            }


            return View(await bookedPatients.ToListAsync());
        }
        public async Task<IActionResult> RetakeVitals()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> RetakeVitals2(int selectedId)
        {


            if (selectedId <= 0)
            {
                _logger.LogWarning("Invalid admission id: {Id}", selectedId);
                return NotFound("Invalid admission ID.");
            }

            var selectedAdmission = await _context.Admissions
                .Include(a => a.Patients)
                .FirstOrDefaultAsync(a => a.AdmissionID == selectedId);

            if (selectedAdmission == null)
            {
                _logger.LogWarning("Admission not found with id: {Id}", selectedId);
                return NotFound("Admission not found.");
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == selectedAdmission.PatientID);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", selectedAdmission.PatientID);
                return NotFound("Patient not found.");
            }

            var model = new VitalsVM
            {
                //AdmissionID = selectedAdmission.AdmissionID,
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = DateTime.Now, // Default to current date for new vitals entry
                Time = TimeSpan.Zero, // Default to current time for new vitals entry
                //Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight,
                //Height = patient.Patient_Vitals.FirstOrDefault()?.Height,
                Vitals = new List<Patient_VitalsVM> // Initialize an empty list for first-time vitals entry
        {
            new Patient_VitalsVM { Vital = "Body Temperature"},
            new Patient_VitalsVM { Vital = "Heart Rate" },
            new Patient_VitalsVM { Vital = "Oxygen Suteration"},

        }
            };

            var vitalsList = await _context.Vitals
       .Select(v => new Patient_VitalsVM
       {
           Vital = v.Vital, // The name of the vital (e.g., Body Temperature, Heart Rate)
           Min = v.Min,
           Max = v.Max,
           Notes = string.Empty // Initialize Notes as empty, can be filled in the view
       }).ToListAsync();

            //Initialize the view model with the necessary data
            //var model = new VitalsVM
            //{
            //    PatientID = patient.PatientID,
            //    Name = patient.Name,
            //    Surname = patient.Surname,
            //    Date = DateTime.Now.Date, // Set default date to current date
            //    Time = DateTime.Now.TimeOfDay, // Set default time to current time
            //    Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight,
            //    Height = patient.Patient_Vitals.FirstOrDefault()?.Height,
            //    Vitals = vitalsList // Populate with the available vitals
            //};

            return View(model);
          
        }

        [HttpPost]
        public async Task<IActionResult> RetakeVitals2(VitalsVM model)
        {
if (!ModelState.IsValid)
    {
        _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
        return View(model);
    }

    try
    {
        // Check if Vitals list is null or empty
        if (model.Vitals == null || !model.Vitals.Any())
        {
            _logger.LogWarning("Vitals list is null or empty.");
            ModelState.AddModelError(string.Empty, "No vitals data provided.");
            return View(model);
        }

        // Extract Date and Time once
        DateTime date = model.Date;
        TimeSpan time = model.Time;

        // Create a list of Patient_Vitals to save
        var vitalsList = new List<Patient_Vitals>();

        foreach (var vital in model.Vitals)
        {
            if (!string.IsNullOrEmpty(vital.Value))
            {
                var patientVital = new Patient_Vitals
                {
                    Date = date,
                    Time = time,
                    Value = vital.Value,
                    VitalsID = await GetVitalsId(vital.Vital),
                    PatientID = model.PatientID,
                    Notes = vital.Notes
                };

                vitalsList.Add(patientVital);
            }
        }

        // Log the data being saved for debugging
        _logger.LogInformation("Saving vitals data: {VitalsData}", JsonConvert.SerializeObject(vitalsList));

        // Save vitals to the database
        _context.Patient_Vitals.AddRange(vitalsList);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Vitals data saved successfully.");

        // Reload patient data to ensure name and details are correct after saving
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientID == model.PatientID);

        if (patient == null)
        {
            _logger.LogWarning("Patient not found with id: {Id}", model.PatientID);
            return RedirectToAction("Error", "Home");
        }

        // Redirect to DisplayVitals with updated patient information
        return RedirectToAction("DisplayVitals", "Nurse", new { selectedId = model.PatientID });
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

        return View(model);
    }

        }

        //Helper method to get VitalsID by vital name
        private async Task<int> GetVitalsId(string vitalName)
        {
            var vital = await _context.Vitals.FirstOrDefaultAsync(v => v.Vital == vitalName);
            if (vital == null)
            {
                throw new Exception($"Vital '{vitalName}' not found in the database.");
            }

            return vital.VitalsID;
        }
        public IActionResult RetakeVitals3()
        {
            return View();
        }
        public IActionResult DisplayVitals3()
        {
            return View();
        }
        public IActionResult AdministeredMeds(int id)
        {
            return View();
        }

        [HttpGet("AdmittedPatients")]
        public async Task<ActionResult> AdmittedPatients(string searchString)
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

        [HttpGet]
        public async Task<IActionResult> AdmitPatient(int selectedSurgeryId)
        {
            var treatmentCode = GetTreatmentCode();

            var selectedSurgery = await _context.Surgeries
                                .Include(s => s.Patient)
                                .Include(s => s.Surgery_TreatmentCodes)
                                .DefaultIfEmpty()
                                .FirstOrDefaultAsync(s => s.SurgeryID == selectedSurgeryId);

          

            if (selectedSurgery == null)
            {
                _logger.LogWarning("Selected surgery is null for SurgeryID: {SurgeryID}", selectedSurgeryId);
                return NotFound();
            }

            if (selectedSurgery.Patient == null)
            {
                _logger.LogWarning("Patient is null for SurgeryID: {SurgeryID}", selectedSurgery.SurgeryID);
                return NotFound();  
            }

            var surgery_treatmentCode = selectedSurgery.Surgery_TreatmentCodes?.Description;

            var model = new AdmissionVM
            {
                Date = DateTime.Now,
                Time = DateTime.Now.ToString("tt"),
                Surgery_TreatmentCode = treatmentCode,
                SurgeryID = selectedSurgery.SurgeryID,
                Name = selectedSurgery.Patient.Name ?? "Unknown",
                Surname = selectedSurgery.Patient.Surname ?? "Unknown",
                Description = selectedSurgery.Surgery_TreatmentCodes.Description

            };
         
            ViewBag.Wards = new SelectList(_context.Ward.ToList(), "WardId", "WardName");
           
            //ViewBag.TreatmentCodes = new SelectList(_context.Ward.ToList(), "Surgery_TreatmentCodeID", "Description");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmitPatient(AdmissionVM model, int selectedSurgeryId)
        {
            //int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);
            var nurseClaim = User.Claims.FirstOrDefault(c => c.Type == "NurseID");
            if (nurseClaim != null && int.TryParse(nurseClaim.Value, out int nurseID))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var selectedSurgery = await _context.Surgeries
                                                    .Include(s => s.Patient)
                                                    .Include(s => s.Surgery_TreatmentCodes)
                                                    .FirstOrDefaultAsync(s => s.SurgeryID == selectedSurgeryId);

                        var patient = selectedSurgery.Patient;

                        if (selectedSurgery?.Patient == null)
                        {
                            _logger.LogWarning("Patient information is missing for SurgeryID: {SurgeryID}", selectedSurgeryId);
                            return NotFound();
                        }

                        int selectedBedId = model.BedId;


                        patient.TreatmentCodeID = model.Surgery_TreatmentCodeID;
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
                        await InsertSpecificTreatmentCode(model.Surgery_TreatmentCodeID, nurseID);

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
                ViewBag.Wards = new SelectList(_context.Ward.ToList(), "WardId", "WardName");
              
                //ViewBag.TreatmentCodes = new SelectList(_context.Surgery_TreatmentCodes.ToList(), "Surgery_TreatmentCodeID", "Description");
            }
            else
            {
                _logger.LogWarning("Nurse ID claim is missing or invalid.");
                ModelState.AddModelError("", "Unable to identify nurse.");
            }

            return View(model);

        }
        public IActionResult Wards()
        {

            ViewBag.Wards = new SelectList(_context.Ward.ToList(), "WardId", "WardName");
            return View();
        }

        public JsonResult GetBeds(int wardId)
        {
            var beds = _context.Bed
        .Where(b => b.WardId == wardId)
        .Select(b => new
        {
            value=b.BedId.ToString(),    // Numeric ID
            b.BedName,
            b.IsAvailable
        })
        .ToList();

            return Json(beds);
        }
        public IActionResult TreatmentCode()
        {

            ViewBag.TreatmentCodes = new SelectList(_context.Surgery_TreatmentCodes.ToList(), "Surgery_TreatmentCodeID", "Description");
            return View();
        }

        private async Task InsertSpecificTreatmentCode(int treatmentCodeId, int userId)
        {
            string treatmentCodeTable = treatmentCodeId switch
            {

                2 => "Gwr0521",
                3 => "Fwo5412",
                4 => "Bsa3164",
                //5 => "Anaesthesiologist",
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
                    string sql = "SELECT TreatmentCodeID, Description FROM [TreatmentCode]";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                treatmentCode.Add(new SelectListItem
                                {
                                    Value = reader["TreatmentCodeID"].ToString(),
                                    Text = reader["Description"].ToString()
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


        
        public async Task<IActionResult> EditTreatmentCode(AdmissionVM model)
        {

            if (!ModelState.IsValid)
            {
                model.Surgery_TreatmentCode = new SelectList(_context.Surgery_TreatmentCodes.ToList(), "Surgery_TreatmentCodeID", "Description", model.Surgery_TreatmentCodeID);
                return View(model);
            }

            
            var surgery = await _context.Surgeries.FindAsync(model.SurgeryID);
            if (surgery == null)
            {
                _logger.LogWarning("Surgery not found with ID: {SurgeryID}", model.SurgeryID);
                return NotFound();
            }

            int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);

            // Update the treatment code
            surgery.Surgery_TreatmentCodeID = model.Surgery_TreatmentCodeID;
           

            try
            {
                _context.Update(surgery);
                await _context.SaveChangesAsync();

                // Debugging: Confirm that the changes are being saved
                _logger.LogInformation("Surgery updated successfully. New Surgery_TreatmentCodeID: {Surgery_TreatmentCodeID}", surgery.Surgery_TreatmentCodeID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating surgery");
                return View(model); // Handle the error appropriately
            }


            return RedirectToAction("AdmitPatient", new { selectedSurgeryId = model.SurgeryID });
        }
        public async Task<ActionResult> PatientsVitals(string searchString)
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




    }
}




