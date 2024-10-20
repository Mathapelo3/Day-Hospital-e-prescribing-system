using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Day_Hospital_e_prescribing_system.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Day_Hospital_e_prescribing_system.ViewModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Day_Hospital_e_prescribing_system.Helper;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;
using System.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Dapper;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    [Authorize]
    public class SurgeonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SurgeonController> _logger;
        private readonly IConfiguration _config;
        private readonly CommonHelper _helper;
        private readonly SurgeriesReportGenerator _surgeriesReportGenerator;

        public SurgeonController(ApplicationDbContext context, ILogger<SurgeonController> logger, IConfiguration config, SurgeriesReportGenerator surgeriesReportGenerator)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _surgeriesReportGenerator = surgeriesReportGenerator;
            _helper = new CommonHelper(_config);
        }

        public ActionResult Dashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        [HttpGet]
        public IActionResult Prescriptions(string id, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID Number is required");
            }

            var viewModel = new PatientPrescriptionViewModel();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("GetPatientPrescriptions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IDNo", id);
                    command.Parameters.AddWithValue("@StartDate", (object)startDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        // Read patient details
                        if (reader.Read())
                        {
                            viewModel.PatientName = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name"));
                            viewModel.PatientSurname = reader.IsDBNull(reader.GetOrdinal("Surname")) ? null : reader.GetString(reader.GetOrdinal("Surname"));
                        }
                        else
                        {
                            //return View("Error", new ErrorViewModel { Message = "No patient found with this ID." });
                        }

                        // Move to next result set (prescriptions)
                        reader.NextResult();

                        // Read prescriptions
                        viewModel.Prescriptions = new List<PatientPrescriptionViewModel.PrescriptionDetails>();
                        while (reader.Read())
                        {
                            viewModel.Prescriptions.Add(new PatientPrescriptionViewModel.PrescriptionDetails
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                InstructionText = reader.IsDBNull(reader.GetOrdinal("InstructionText")) ? null : reader.GetString(reader.GetOrdinal("InstructionText")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Urgency = reader.GetBoolean(reader.GetOrdinal("Urgency")),
                                MedicationName = reader.IsDBNull(reader.GetOrdinal("MedicationName")) ? null : reader.GetString(reader.GetOrdinal("MedicationName")),
                            });
                        }
                    }
                }
            }

            if (viewModel.Prescriptions.Count == 0)
            {
                ViewBag.Message = "No prescriptions found for this patient within the specified date range.";
            }

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(viewModel);
        }

        public async Task<ActionResult> Patients(string searchString)
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

        public IActionResult AddPatients()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPatients(AddPatientsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("dbo.AddPatient", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Name", model.Name));
                        command.Parameters.Add(new SqlParameter("@Surname", model.Surname));
                        command.Parameters.Add(new SqlParameter("@Email", model.Email));
                        command.Parameters.Add(new SqlParameter("@IDNo", model.IDNo));
                        command.Parameters.Add(new SqlParameter("@Gender", model.Gender));
                        command.Parameters.Add(new SqlParameter("@Status", model.Status));

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("Patients", "Surgeon");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding the patient: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Surgeries(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            // Get the logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Convert userId to int if necessary
            int surgeonId;
            if (!int.TryParse(userId, out surgeonId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get the surgeon's ID based on the user ID
            var surgeon = await _context.Surgeons
                .FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);

            if (surgeon == null)
            {
                return NotFound("Surgeon not found");
            }

            // Call the stored procedure
            var surgeryDetails = await _context.Set<SurgeryDetailsViewModel>().FromSqlRaw(
                "EXEC [dbo].[GetBookedSurgeryDetails] @StartDate, @EndDate, @SurgeonID",
                new SqlParameter("@StartDate", startDate ?? (object)DBNull.Value),
                new SqlParameter("@EndDate", endDate ?? (object)DBNull.Value),
                new SqlParameter("@SurgeonID", surgeon.SurgeonID)
            ).ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(surgeryDetails);
        }


        [HttpGet]
        public IActionResult GenerateSurgeriesReport(DateTime startDate, DateTime endDate)
        {
            var surgeonName = HttpContext.Session.GetString("Name");
            var surgeonSurname = HttpContext.Session.GetString("Surname");

            if (string.IsNullOrEmpty(surgeonName) || string.IsNullOrEmpty(surgeonSurname))
            {
                _logger.LogWarning("Surgeon name or surname could not be retrieved from the session.");
                return BadRequest("Unable to retrieve surgeon details.");
            }

            var reportStream = _surgeriesReportGenerator.GenerateSurgeriesReport(startDate, endDate, surgeonName, surgeonSurname);

            // Ensure the stream is not disposed prematurely
            if (reportStream == null || reportStream.Length == 0)
            {
                return NotFound(); // Or handle as appropriate
            }

            // Return the PDF file
            return File(reportStream, "application/pdf", "SurgeriesReport.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> AllPrescriptions(DateTime? filterDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Get the logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Convert userId to int
            if (!int.TryParse(userId, out int surgeonId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get the surgeon's ID based on the user ID
            var surgeon = await _context.Surgeons
                .FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);
            if (surgeon == null)
            {
                return NotFound("Surgeon not found");
            }

            // Execute the stored procedure and manually map the results
            var prescriptions = new List<AllPrescriptionsViewModel>();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "GetPrescriptionsForAllPatients";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@FilterDate", SqlDbType.Date) { Value = (object)filterDate ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@SurgeonID", SqlDbType.Int) { Value = surgeon.SurgeonID });

                await _context.Database.OpenConnectionAsync();

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        prescriptions.Add(new AllPrescriptionsViewModel
                        {
                            PrescriptionID = result.GetInt32(result.GetOrdinal("PrescriptionID")),
                            PatientID = result.GetInt32(result.GetOrdinal("PatientID")),
                            SurgeonID = result.GetInt32(result.GetOrdinal("SurgeonID")),
                            StockID = result.IsDBNull(result.GetOrdinal("StockID")) ? null : (int?)result.GetInt32(result.GetOrdinal("StockID")),
                            PatientName = result.GetString(result.GetOrdinal("PatientName")),
                            PatientSurname = result.GetString(result.GetOrdinal("PatientSurname")),
                            SurgeonName = result.GetString(result.GetOrdinal("SurgeonName")),
                            SurgeonSurname = result.GetString(result.GetOrdinal("SurgeonSurname")),
                            MedicationName = result.IsDBNull(result.GetOrdinal("MedicationName")) ? null : result.GetString(result.GetOrdinal("MedicationName")),
                            Date = result.GetDateTime(result.GetOrdinal("Date")),
                            InstructionText = result.IsDBNull(result.GetOrdinal("InstructionText")) ? null : result.GetString(result.GetOrdinal("InstructionText")),
                            Quantity = result.IsDBNull(result.GetOrdinal("Quantity")) ? null : (int?)result.GetInt32(result.GetOrdinal("Quantity")),
                            Status = result.GetString(result.GetOrdinal("Status")),
                            Urgency = result.GetBoolean(result.GetOrdinal("Urgency"))
                        });
                    }
                }
            }

            ViewBag.FilterDate = filterDate;
            return View(prescriptions);
        }
    

        [HttpGet]
        public IActionResult NewPrescription()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var viewModel = new NewPatientPrescriptionViewModel
            {
                PatientList = _context.Patients
                    .OrderBy(p => p.Surname)
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                    }).ToList(),

                MedicationList = _context.DayHospitalMedication
                    .OrderBy(m => m.MedicationName)
                    .Where(m => m.MedicationName != null)
                    .Select(m => new SelectListItem
                    {
                        Value = m.StockID.ToString(),
                        Text = m.MedicationName
                    }).ToList(),
                Date = DateTime.Today
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CheckAllergy(int patientId, int stockId)
        {
            var parameter = new SqlParameter("@AlertMessage", SqlDbType.NVarChar, -1)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.AllergyCheckForSurgeon @PatientID, @StockID, @AlertMessage OUT",
                new SqlParameter("@PatientID", patientId),
                new SqlParameter("@StockID", stockId),
                parameter
            );

            string alertMessage = parameter.Value == DBNull.Value ? null : (string)parameter.Value;

            return Json(new { hasAllergy = !string.IsNullOrEmpty(alertMessage), message = alertMessage });
        }

        // Modify your existing NewPrescription action
        [HttpPost]
        public async Task<IActionResult> NewPrescription(NewPatientPrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(model);
                return View(model);
            }

            try
            {
                if (model.SelectedMedications == null || !model.SelectedMedications.Any())
                {
                    ModelState.AddModelError(string.Empty, "Please select at least one medication.");
                    PopulateDropdownLists(model);
                    return View(model);
                }

                // Check for allergies one final time before saving
                foreach (var medication in model.SelectedMedications)
                {
                    var parameter = new SqlParameter("@AlertMessage", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC dbo.AllergyCheckForSurgeon @PatientID, @StockID, @AlertMessage OUT",
                        new SqlParameter("@PatientID", model.SelectedPatientId),
                        new SqlParameter("@StockID", medication.StockID),
                        parameter
                    );

                    string alertMessage = parameter.Value == DBNull.Value ? null : (string)parameter.Value;
                    if (!string.IsNullOrEmpty(alertMessage))
                    {
                        ModelState.AddModelError(string.Empty,
                            $"Cannot proceed: {alertMessage}");
                        PopulateDropdownLists(model);
                        return View(model);
                    }
                }

                var medicationData = JsonConvert.SerializeObject(model.SelectedMedications);
                var surgeonId = GetLoggedInSurgeonId();

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.NewPrescription @PatientID, @SurgeonID, @Date, @Urgency, @MedicationData",
                    new SqlParameter("@PatientID", model.SelectedPatientId),
                    new SqlParameter("@SurgeonID", surgeonId),
                    new SqlParameter("@Date", model.Date),
                    new SqlParameter("@Urgency", model.Urgency),
                    new SqlParameter("@MedicationData", medicationData)
                );

                await _context.SaveChangesAsync();
                return RedirectToAction("AllPrescriptions", new { id = model.SelectedPatientId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                PopulateDropdownLists(model);
                return View(model);
            }
        }

        private void PopulateDropdownLists(NewPatientPrescriptionViewModel model)
        {
            model.PatientList = _context.Patients
                .OrderBy(p => p.Surname)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientID.ToString(),
                    Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                }).ToList();

            model.MedicationList = _context.DayHospitalMedication
                .OrderBy(m => m.MedicationName)
                .Where(m => m.MedicationName != null)
                .Select(m => new SelectListItem
                {
                    Value = m.StockID.ToString(),
                    Text = m.MedicationName
                }).ToList();
        }

        private int GetLoggedInSurgeonId()
        {
            // Implement this method to get the logged-in surgeon's ID
            // This could be from a claim, session, or database lookup
            // For example:
            var username = HttpContext.Session.GetString("Username");
            var surgeon = _context.Surgeons.FirstOrDefault(s => s.User.Username == username);
            return surgeon?.SurgeonID ?? throw new InvalidOperationException("Logged in surgeon not found");
        }

        //public async Task<IActionResult> EditPrescription(int? id)
        //{
        //    ViewBag.Username = HttpContext.Session.GetString("Username");

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var prescription = await _context.Prescriptions.FindAsync(id);
        //    if (prescription == null)
        //    {
        //        return NotFound();
        //    }

        //    var viewModel = new PrescriptionViewModel
        //    {
        //        PrescriptionID = prescription.PrescriptionID,
        //        Instruction = prescription.Instruction,
        //        Date = prescription.Date,
        //        Quantity = prescription.Quantity,
        //        Status = prescription.Status,
        //        Urgency = prescription.Urgency
        //    };

        //    return View(viewModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditPrescription(int id, PrescriptionViewModel model)
        //{
        //    if (id != model.PrescriptionID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var prescription = await _context.Prescriptions.FindAsync(id);
        //            if (prescription == null)
        //            {
        //                return NotFound();
        //            }

        //            prescription.Instruction = model.Instruction;
        //            prescription.Date = model.Date;
        //            prescription.Quantity = model.Quantity;
        //            prescription.Status = model.Status;
        //            prescription.Urgency = model.Urgency;

        //            _context.Update(prescription);
        //            await _context.SaveChangesAsync();
        //            _logger.LogInformation("Prescription record updated successfully.");
        //            return RedirectToAction("Prescriptions", "Surgeon");
        //        }
        //        catch (DbUpdateException ex)
        //        {
        //            _logger.LogError(ex, "An error occurred while updating the prescription record: {Message}", ex.Message);
        //            ModelState.AddModelError("", "Unable to save changes.");
        //        }
        //    }
        //    else
        //    {
        //        _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //    }

        //    return View(model);
        //}

        public IActionResult NewSurgery()
        {
            var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Logged-in user's email: {Email}", loggedInUserEmail);

            if (string.IsNullOrEmpty(loggedInUserEmail))
            {
                _logger.LogError("Logged-in user's email is null or empty.");
                return RedirectToAction("Error", "Home");
            }

            // Get the surgeon ID for the logged-in user
            var surgeonID = _helper.GetSurgeonByEmail(
                "SELECT s.SurgeonID, s.UserID, u.Username, u.Name, u.Surname " +
                "FROM Surgeon s INNER JOIN [User] u ON s.UserID = u.UserID " +
                "WHERE u.Email = @Email",
                loggedInUserEmail).SurgeonID;

            ViewBag.Username = HttpContext.Session.GetString("Username");
            var viewModel = new SurgeryViewModel
            {
                // Pre-set the SurgeonID from the logged-in user
                SurgeonID = surgeonID,

                PatientList = _context.Patients
                    .OrderBy(p => p.Surname)
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                    }).ToList(),
                // Only include the logged-in surgeon in the list
                SurgeonList = _context.Surgeons
                    .Where(s => s.SurgeonID == surgeonID)
                    .Select(s => new SelectListItem
                    {
                        Value = s.SurgeonID.ToString(),
                        Text = $"{s.User.Name} {s.User.Surname}",
                        Selected = true
                    }).ToList(),

                AnaesthesiologistList = _context.Anaesthesiologists
                    .OrderBy(a => a.User.Surname)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AnaesthesiologistID.ToString(),
                        Text = $"{a.User.Name} {a.User.Surname}"
                    }).ToList(),

                TheatreList = _context.Theatres
                    .OrderBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TheatreID.ToString(),
                        Text = t.Name
                    }).ToList(),

                TreatmentCodeList = _context.TreatmentCodes
                    .OrderBy(tc => tc.ICD_10_Code)
                    .Select(tc => new SelectListItem
                    {
                        Value = tc.TreatmentCodeID.ToString(),
                        Text = $"{tc.ICD_10_Code} - {tc.Description}"
                    }).ToList(),
                Date = DateTime.Today
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> NewSurgery(SurgeryViewModel model)
        {
            var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
            _logger.LogInformation("Logged-in user's email: {Email}", loggedInUserEmail);

            if (string.IsNullOrEmpty(loggedInUserEmail))
            {
                _logger.LogError("Logged-in user's email is null or empty.");
                return RedirectToAction("Error", "Home");
            }

            // Get the surgeon ID for the logged-in user
            var surgeonID = _helper.GetSurgeonByEmail(
                "SELECT s.SurgeonID, s.UserID, u.Username, u.Name, u.Surname " +
                "FROM Surgeon s INNER JOIN [User] u ON s.UserID = u.UserID " +
                "WHERE u.Email = @Email",
                loggedInUserEmail).SurgeonID;

            // Ensure the surgery is being created for the logged-in surgeon
            model.SurgeonID = surgeonID;

            // Log the state of SelectedTreatmentCodes
            _logger.LogInformation("SelectedTreatmentCodes: {@SelectedTreatmentCodes}", model.SelectedTreatmentCodes);

            try
            {
                // Validate selected treatment codes
                if (model.SelectedTreatmentCodes == null || !model.SelectedTreatmentCodes.Any())
                {
                    _logger.LogWarning("No treatment codes selected");
                    ModelState.AddModelError(string.Empty, "Please select at least one treatment code.");
                    PopulateDropdownLists(model);
                    return View(model);
                }

                // Convert SelectedTreatmentCodes to a comma-separated string
                var treatmentCodesData = string.Join(",", model.SelectedTreatmentCodes);
                _logger.LogInformation("Treatment codes data: {TreatmentCodesData}", treatmentCodesData);

                // Execute stored procedure
                var newSurgeryIdParam = new SqlParameter("@NewSurgeryID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.AddNewSurgery @Date, @Time, @PatientID, @SurgeonID, @AnaesthesiologistID, @TheatreID, @TreatmentCodes, @NewSurgeryID OUTPUT",
                    new SqlParameter("@Date", model.Date),
                    new SqlParameter("@Time", model.Time),
                    new SqlParameter("@PatientID", model.PatientID),
                    new SqlParameter("@SurgeonID", surgeonID),
                    new SqlParameter("@AnaesthesiologistID", model.AnaesthesiologistID),
                    new SqlParameter("@TheatreID", model.TheatreID),
                    new SqlParameter("@TreatmentCodes", treatmentCodesData),
                    newSurgeryIdParam
                );

                int newSurgeryId = (int)newSurgeryIdParam.Value;
                _logger.LogInformation("New surgery created with ID: {NewSurgeryId}", newSurgeryId);

                return RedirectToAction("Surgeries", new { id = newSurgeryId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new surgery");
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                PopulateDropdownLists(model);
                return View(model);
            }
        }

        private void PopulateDropdownLists(SurgeryViewModel model)
        {
            // Get the logged-in surgeon's ID
            var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var surgeonID = _helper.GetSurgeonByEmail(
                "SELECT s.SurgeonID, s.UserID, u.Username, u.Name, u.Surname " +
                "FROM Surgeon s INNER JOIN [User] u ON s.UserID = u.UserID " +
                "WHERE u.Email = @Email",
                loggedInUserEmail).SurgeonID;

            model.PatientList = _context.Patients
                .OrderBy(p => p.Surname)
                .Select(p => new SelectListItem
                {
                    Value = p.PatientID.ToString(),
                    Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                }).ToList();

            // Only include the logged-in surgeon in the list
            model.SurgeonList = _context.Surgeons
                .Where(s => s.SurgeonID == surgeonID)
                .Select(s => new SelectListItem
                {
                    Value = s.SurgeonID.ToString(),
                    Text = $"{s.User.Name} {s.User.Surname}",
                    Selected = true
                }).ToList();

            model.AnaesthesiologistList = _context.Anaesthesiologists
                .OrderBy(a => a.User.Surname)
                .Select(a => new SelectListItem
                {
                    Value = a.AnaesthesiologistID.ToString(),
                    Text = $"{a.User.Name} {a.User.Surname}"
                }).ToList();

            model.TheatreList = _context.Theatres
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.TheatreID.ToString(),
                    Text = t.Name
                }).ToList();

            model.TreatmentCodeList = _context.TreatmentCodes
                .OrderBy(tc => tc.ICD_10_Code)
                .Select(tc => new SelectListItem
                {
                    Value = tc.TreatmentCodeID.ToString(),
                    Text = $"{tc.ICD_10_Code} - {tc.Description}"
                }).ToList();
        }

        [HttpGet]
        public IActionResult PatientRecord(string id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var viewModel = new PatientRecordViewModel();
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (var command = new SqlCommand("GetPatientVitalsAllergiesMedicationsAndConditions", connection))
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
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? DateTime.MinValue: DateTime.Parse(reader.GetString(reader.GetOrdinal("DateOfBirth"))),
                                Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender")),
                                ContactNo = reader.IsDBNull(reader.GetOrdinal("ContactNo")) ? null : reader.GetString(reader.GetOrdinal("ContactNo"))
                            };
                        }

                        // Move to next result set (vitals)
                        reader.NextResult();
                        // Read vitals
                        viewModel.Patient_Vitals = new List<Patient_Vitals>();
                        while (reader.Read())
                        {
                            viewModel.Patient_Vitals.Add(new Patient_Vitals
                            {
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Time = reader.GetTimeSpan(reader.GetOrdinal("Time")),
                                Value =  reader.GetString(reader.GetOrdinal("Value"))
                            });

                        }
                        viewModel.Patient_Vitals = viewModel.Patient_Vitals.OrderBy(v => v.Date).ToList();

                        // Move to next result set (allergies)
                        reader.NextResult();
                        // Read allergies
                        viewModel.Allergies = new List<Allergy>();
                        while (reader.Read())
                        {
                            viewModel.Allergies.Add(new Allergy
                            {
                                Name = reader.IsDBNull(reader.GetOrdinal("AllergyName")) ? null : reader.GetString(reader.GetOrdinal("AllergyName")),
                                Description = reader.IsDBNull(reader.GetOrdinal("AllergyDescription")) ? null : reader.GetString(reader.GetOrdinal("AllergyDescription"))
                            });
                        }
                        viewModel.Allergies = viewModel.Allergies.OrderBy(a => a.Name)
                                       .ThenByDescending(a => a.Description)
                                       .ToList();

                        // Move to next result set (conditions)
                        reader.NextResult();
                        // Read conditions
                        viewModel.Conditions = new List<Condition>();
                        while (reader.Read())
                        {
                            viewModel.Conditions.Add(new Condition
                            {
                                Name = reader.GetString(reader.GetOrdinal("ConditionName")),
                                ICD_10_Code = reader.GetString(reader.GetOrdinal("ICD_10_Code"))
                            });
                        }
                        viewModel.Conditions = viewModel.Conditions.OrderBy(c => c.Name).ToList();
                    }
                }
            }
            return View(viewModel);
        }


        public async Task<ActionResult> DischargePatient(string searchString)
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
                patient = patient.Where(p => p.IDNo.Contains(searchString) && p.Status == "Discharge");
            }
            else
            {
                patient = patient.Where(p => p.Status == "Discharge");
            }
            return View(await patient.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> AdmittedPatients(string searchString = null, DateTime? date = null)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            // Get the logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Convert userId to int if necessary
            int surgeonId;
            if (!int.TryParse(userId, out surgeonId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get the surgeon's ID based on the user ID
            var surgeon = await _context.Surgeons
                .FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);

            if (surgeon == null)
            {
                return NotFound("Surgeon not found");
            }

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@SearchString", searchString);
                parameters.Add("@Date", date);
                parameters.Add("@SurgeonID", surgeonId);

                var patients = await connection.QueryAsync<SurgeonPatientsViewModel>(
                    "sp_SurgeonAdmittedPatients",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Store the current filter in ViewData for the view
                ViewData["CurrentFilter"] = searchString;
                ViewData["CurrentDate"] = date?.ToString("yyyy-MM-dd");

                return View(patients);
            }
        }

        //public IActionResult ConfirmTreatmentCodes()
        //{
        //    ViewBag.Username = HttpContext.Session.GetString("Username");

        //    return View();
        //}

    }
}
