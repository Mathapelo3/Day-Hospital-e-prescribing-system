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

namespace Day_Hospital_e_prescribing_system.Controllers
{
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
        public IActionResult Surgeries(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var surgeries = new List<SurgeryDetailsViewModel>();

            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "GetBookedSurgeryDetails";
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date)
                        {
                            Value = startDate.HasValue ? (object)startDate.Value.Date : DBNull.Value
                        });
                        command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date)
                        {
                            Value = endDate.HasValue ? (object)endDate.Value.Date : DBNull.Value
                        });

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                surgeries.Add(new SurgeryDetailsViewModel
                                {
                                    SurgeryID = reader.GetInt32(reader.GetOrdinal("SurgeryID")),
                                    PatientID = reader.IsDBNull(reader.GetOrdinal("PatientID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("PatientID")),
                                    AnaesthesiologistID = reader.IsDBNull(reader.GetOrdinal("AnaesthesiologistID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AnaesthesiologistID")),
                                    TheatreID = reader.IsDBNull(reader.GetOrdinal("TheatreID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("TheatreID")),
                                    PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                    PatientSurname = reader.GetString(reader.GetOrdinal("PatientSurname")),
                                    AnaesthesiologistName = reader.IsDBNull(reader.GetOrdinal("AnaesthesiologistName")) ? null : reader.GetString(reader.GetOrdinal("AnaesthesiologistName")),
                                    AnaesthesiologistSurname = reader.IsDBNull(reader.GetOrdinal("AnaesthesiologistSurname")) ? null : reader.GetString(reader.GetOrdinal("AnaesthesiologistSurname")),
                                    TheatreName = reader.IsDBNull(reader.GetOrdinal("TheatreName")) ? null : reader.GetString(reader.GetOrdinal("TheatreName")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Time = reader.GetString(reader.GetOrdinal("Time")),
                                    ICD_10_Code = reader.IsDBNull(reader.GetOrdinal("ICD_10_Code")) ? null : reader.GetString(reader.GetOrdinal("ICD_10_Code")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                // You might want to add proper logging here
                return View("Error");
            }

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(surgeries);
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
        public IActionResult AllPrescriptions(DateTime? filterDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var prescriptions = new List<AllPrescriptionsViewModel>();

            using (var connection = _context.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "GetPrescriptionsForAllPatients";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@FilterDate";
                    parameter.Value = filterDate.HasValue ? (object)filterDate.Value.Date : DBNull.Value;
                    command.Parameters.Add(parameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prescriptions.Add(new AllPrescriptionsViewModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                SurgeonID = reader.GetInt32(reader.GetOrdinal("SurgeonID")),
                                StockID = reader.IsDBNull(reader.GetOrdinal("StockID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("StockID")),
                                PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                PatientSurname = reader.GetString(reader.GetOrdinal("PatientSurname")),
                                SurgeonName = reader.IsDBNull(reader.GetOrdinal("SurgeonName")) ? null : reader.GetString(reader.GetOrdinal("SurgeonName")),
                                SurgeonSurname = reader.IsDBNull(reader.GetOrdinal("SurgeonSurname")) ? null : reader.GetString(reader.GetOrdinal("SurgeonSurname")),
                                MedicationName = reader.IsDBNull(reader.GetOrdinal("MedicationName")) ? null : reader.GetString(reader.GetOrdinal("MedicationName")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                InstructionText = reader.IsDBNull(reader.GetOrdinal("InstructionText")) ? null : reader.GetString(reader.GetOrdinal("InstructionText")),
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Urgency = reader.GetBoolean(reader.GetOrdinal("Urgency"))
                            });
                        }
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
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                    }).ToList(),
                SurgeonList = _context.Surgeons
                    .Select(p => new SelectListItem
                    {
                        Value = p.SurgeonID.ToString(),
                        Text = $"{p.User.Name} {p.User.Surname}"
                    }).ToList(),
                MedicationList = _context.DayHospitalMedication
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
        public async Task<IActionResult> NewPrescription(NewPatientPrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(model);
                return View(model);
            }
            try
            {
                // Validate selected medications
                if (model.SelectedMedications == null || !model.SelectedMedications.Any())
                {
                    ModelState.AddModelError(string.Empty, "Please select at least one medication.");
                    PopulateDropdownLists(model);
                    return View(model);
                }
                // Serialize selected medications to JSON
                var medicationData = JsonConvert.SerializeObject(model.SelectedMedications);

                // Execute stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.NewPrescription @PatientID, @SurgeonID, @Date, @Urgency, @MedicationData",
                    new SqlParameter("@PatientID", model.SelectedPatientId),
                    new SqlParameter("@SurgeonID", model.SelectedSurgeonId),
                    new SqlParameter("@Date", model.Date),
                    new SqlParameter("@Urgency", model.Urgency),
                    new SqlParameter("@MedicationData", medicationData)
                );

                // Add this line to save changes
                await _context.SaveChangesAsync();

                // If successful, redirect to AllPrescriptions page
                return RedirectToAction("AllPrescriptions", new { id = model.SelectedPatientId });
            }
            catch (Exception ex)
            {
                // Log exception details here
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                PopulateDropdownLists(model);
                return View(model);
            }
        }

        private void PopulateDropdownLists(NewPatientPrescriptionViewModel model)
        {
            model.PatientList = _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientID.ToString(),
                    Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                }).ToList();
            model.MedicationList = _context.DayHospitalMedication
                .Where(m => m.MedicationName != null)
                .Select(m => new SelectListItem
                {
                    Value = m.StockID.ToString(),
                    Text = m.MedicationName
                }).ToList();
            model.SurgeonList = _context.Surgeons
                .Select(p => new SelectListItem
                {
                    Value = p.SurgeonID.ToString(),
                    Text = $"{p.User.Name} {p.User.Surname}"
                }).ToList();
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
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var viewModel = new SurgeryViewModel
            {
                PatientList = _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                    }).ToList(),
                SurgeonList = _context.Surgeons
                    .Select(s => new SelectListItem
                    {
                        Value = s.SurgeonID.ToString(),
                        Text = $"{s.User.Name} {s.User.Surname}"
                    }).ToList(),
                AnaesthesiologistList = _context.Anaesthesiologists
                    .Select(a => new SelectListItem
                    {
                        Value = a.AnaesthesiologistID.ToString(),
                        Text = $"{a.User.Name} {a.User.Surname}"
                    }).ToList(),
                TheatreList = _context.Theatres
                    .Select(t => new SelectListItem
                    {
                        Value = t.TheatreID.ToString(),
                        Text = t.Name
                    }).ToList(),
                TreatmentCodeList = _context.TreatmentCodes
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
            if (!ModelState.IsValid)
            {
                PopulateDropdownLists(model);
                return View(model);
            }

            try
            {
                // Validate selected treatment codes
                if (model.SelectedTreatmentCodes == null || !model.SelectedTreatmentCodes.Any())
                {
                    ModelState.AddModelError(string.Empty, "Please select at least one treatment code.");
                    PopulateDropdownLists(model);
                    return View(model);
                }

                // Convert SelectedTreatmentCodes to the expected format
                var treatmentCodesData = JsonConvert.SerializeObject(
                    model.SelectedTreatmentCodes.Select(tc => new { TreatmentCodeID = int.Parse(tc) })
                );

                // Execute stored procedure
                var newSurgeryIdParam = new SqlParameter("@NewSurgeryID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.AddNewSurgery @Date, @Time, @PatientID, @SurgeonID, @AnaesthesiologistID, @TheatreID, @TreatmentCodesData, @NewSurgeryID OUTPUT",
                    new SqlParameter("@Date", model.Date),
                    new SqlParameter("@Time", model.Time),
                    new SqlParameter("@PatientID", model.PatientID),
                    new SqlParameter("@SurgeonID", model.SurgeonID),
                    new SqlParameter("@AnaesthesiologistID", model.AnaesthesiologistID),
                    new SqlParameter("@TheatreID", model.TheatreID),
                    new SqlParameter("@TreatmentCodesData", treatmentCodesData),
                    newSurgeryIdParam
                );

                int newSurgeryId = (int)newSurgeryIdParam.Value;

                // If successful, redirect to a details or list page
                return RedirectToAction("SurgeryDetails", new { id = newSurgeryId });
            }
            catch (Exception ex)
            {
                // Log exception details here
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                PopulateDropdownLists(model);
                return View(model);
            }
        }

        private void PopulateDropdownLists(SurgeryViewModel model)
        {
            model.PatientList = _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.PatientID.ToString(),
                    Text = $"{p.Name} {p.Surname} - ({p.IDNo})"
                }).ToList();
            model.SurgeonList = _context.Surgeons
                .Select(s => new SelectListItem
                {
                    Value = s.SurgeonID.ToString(),
                    Text = $"{s.User.Name} {s.User.Surname}"
                }).ToList();
            model.AnaesthesiologistList = _context.Anaesthesiologists
                .Select(a => new SelectListItem
                {
                    Value = a.AnaesthesiologistID.ToString(),
                    Text = $"{a.User.Name} {a.User.Surname}"
                }).ToList();
            model.TheatreList = _context.Theatres
                .Select(t => new SelectListItem
                {
                    Value = t.TheatreID.ToString(),
                    Text = t.Name
                }).ToList();
            model.TreatmentCodeList = _context.TreatmentCodes
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
                        viewModel.Vitals = new List<Vitals>();
                        while (reader.Read())
                        {
                            viewModel.Vitals.Add(new Vitals
                            {
                                Vital = reader.GetString(reader.GetOrdinal("Vital")),
                                Min = reader.GetString(reader.GetOrdinal("Min")),
                                Max = reader.GetString(reader.GetOrdinal("Max"))
                            });
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
                                Description = reader.GetString(reader.GetOrdinal("AllergyDescription"))
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
                                ICD_10_Code = reader.GetString(reader.GetOrdinal("ICD_10_Code"))
                            });
                        }
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
        public async Task<ActionResult> AdmittedPatients(string searchString)
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
                patient = patient.Where(p => p.IDNo.Contains(searchString) && p.Status == "Admitted");
            }
            else
            {
                patient = patient.Where(p => p.Status == "Admitted");
            }
            return View(await patient.ToListAsync());
        }

        //public IActionResult ConfirmTreatmentCodes()
        //{
        //    ViewBag.Username = HttpContext.Session.GetString("Username");

        //    return View();
        //}

    }
}
