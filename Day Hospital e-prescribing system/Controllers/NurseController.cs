using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;



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
        public IActionResult Vitals()
        {
            return View();
        }
        public IActionResult DisplayVitals()
        {
            return View();
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

            string query = "INSERT INTO [Patient_Allergy] (Name) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@Name)" +
                            "INSERT INTO [Patient_Condition] (Name) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@Name)" +
                            "INSERT INTO [Patient_Medication] (Name) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@Name)";





            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Allergy_Name);
                        command.Parameters.AddWithValue("@Name", model.Condition_Name);
                        command.Parameters.AddWithValue("@Name", model.Meds_Name);
                        //command.Parameters.AddWithValue("@Date", model.Date);
                        //command.Parameters.AddWithValue("@Time", model.Time);

                        int userId = (int)await command.ExecuteScalarAsync();

                        await InsertSpecificAllergy(model.AllergyID, userId);
                        await InsertSpecificCondition(model.ConditionID, userId);
                        await InsertSpecificGeneral_Medication(model.General_MedicationID, userId);

                        //TempData["SuccessMessage"] = "User successfully added into the system.";
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


        public async Task<ActionResult> AddCodition2(int Id, string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var searchpatient = from s in _context.Patients
                                select s;
            if (!System.String.IsNullOrEmpty(searchString))
            {
                searchpatient = searchpatient.Where(s => s.Name.Contains(searchString)
                                       || s.Surname.Contains(searchString));
            }

            var patient = await _context.Patients
                 .Include(p => p.Patient_Allergy)
                 .ThenInclude(pa => pa.Allergy)
                 .Include(p => p.Patient_Condition)
                 .ThenInclude(pc => pc.Condition)
                 .Include(p => p.Patient_Medication)
                 .ThenInclude(pm => pm.General_Medication)
                 .FirstOrDefaultAsync(p => p.PatientID == Id);


            return View(patient);


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
            if (String.IsNullOrEmpty(searchString))
            {
                searchpatient = searchpatient.Where(s => s.Name.Contains(searchString)
                                       || s.Surname.Contains(searchString));
            }
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
                                     Patient = $"{p.Name} {p.Surname}",
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
        public IActionResult RetakeVitals()
        {
            return View();
        }
        public IActionResult RetakeVitals2()
        {
            return View();
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
        public IActionResult AdmittedPatients()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AdmitPatient()
        {
            var treatmentCode = GetTreatmentCode();

            var model = new AdmissionVM
            {
                TreatmentCode = treatmentCode
            };

            ViewBag.Wards = new SelectList(_context.Ward.ToList(), "WardId", "WardName");

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AdmitPatient(AdmissionVM model)
        {
            if (!ModelState.IsValid)
            {
                model.TreatmentCode = GetTreatmentCode();
                ViewBag.Wards = new SelectList(_context.Ward.ToList(), "WardId", "WardName");
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return View(model);
            }


            int nurseID = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "NurseID").Value);

            string query = "INSERT INTO [Admission] (WardId, BedId, TreatmentCodeID, Date, Time) " +
                           "OUTPUT INSERTED.UserID " +
                           "VALUES (@WardId, @BedId, @TreatmentCodeID, @Date, @Time";



            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@WardId", model.WardId);
                        command.Parameters.AddWithValue("@BedId", model.BedId);
                        command.Parameters.AddWithValue("@TreatmentCodeID", model.TreatmentCodeID);
                        command.Parameters.AddWithValue("@Date", model.Date);
                        command.Parameters.AddWithValue("@Time", model.Time);

                        int userId = (int)await command.ExecuteScalarAsync();


                        await InsertSpecificTreatmentCode(model.TreatmentCodeID, userId);

                        //TempData["SuccessMessage"] = "User successfully added into the system.";
                        return RedirectToAction("Wards");
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
                              b.BedId,
                              b.BedName,
                              b.IsAvailable
                          })
                          .ToList();

            return Json(beds);
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

    }
}




