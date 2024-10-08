﻿using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Prescriptions(string id)
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
                            // Handle case where no patient found
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
                                Instruction = reader.GetString(reader.GetOrdinal("Instruction")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Quantity = reader.GetString(reader.GetOrdinal("Quantity")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Urgency = reader.GetBoolean(reader.GetOrdinal("Urgency")),
                                GeneralMedicationName = reader.GetString(reader.GetOrdinal("GeneralMedicationName")),
                            });
                        }

                        // Check if there are any prescriptions in the debug result set
                        reader.NextResult();
                        if (!reader.HasRows)
                        {
                            // Handle case where no prescriptions found
                            ViewBag.DebugMessage = "No prescriptions found for this patient.";
                        }
                    }
                }
            }

            if (viewModel.Prescriptions.Count == 0)
            {
                ViewBag.Message = "No prescriptions found for this patient.";
            }

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
        public async Task<ActionResult> Surgeries(DateTime? startDate, DateTime? endDate)
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
            foreach (var surgery in surgeries)
            {
                surgery.SurgeryCodes = surgery.SurgeryCode?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }

            // Pass the date values to the view for maintaining filter state
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

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

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC GetPrescriptionsForAllPatients @FilterDate";
                command.CommandType = System.Data.CommandType.Text;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@FilterDate";
                parameter.Value = (object)filterDate ?? DBNull.Value;
                command.Parameters.Add(parameter);

                _context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        prescriptions.Add(new AllPrescriptionsViewModel
                        {
                            PrescriptionID = result.GetInt32(result.GetOrdinal("PrescriptionID")),
                            PatientID = result.GetInt32(result.GetOrdinal("PatientID")),
                            SurgeonID = result.GetInt32(result.GetOrdinal("SurgeonID")),
                            MedicationID = result.IsDBNull(result.GetOrdinal("MedicationID")) ? (int?)null : result.GetInt32(result.GetOrdinal("MedicationID")),
                            PatientName = result.GetString(result.GetOrdinal("PatientName")),
                            PatientSurname = result.GetString(result.GetOrdinal("PatientSurname")),
                            SurgeonName = result.IsDBNull(result.GetOrdinal("SurgeonName")) ? null : result.GetString(result.GetOrdinal("SurgeonName")),
                            SurgeonSurname = result.IsDBNull(result.GetOrdinal("SurgeonSurname")) ? null : result.GetString(result.GetOrdinal("SurgeonSurname")),
                            MedicationName = result.IsDBNull(result.GetOrdinal("MedicationName")) ? null : result.GetString(result.GetOrdinal("MedicationName")),
                            Date = result.GetDateTime(result.GetOrdinal("Date")),
                            Instruction = result.GetString(result.GetOrdinal("Instruction")),
                            Quantity = result.GetString(result.GetOrdinal("Quantity")),
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

                // Serialize selected treatment codes to JSON
                var treatmentCodesData = JsonConvert.SerializeObject(model.SelectedTreatmentCodes.Select(tc => new { TreatmentCodeID = tc }));

                // Execute stored procedure
                var newSurgeryIdParam = new SqlParameter("@NewSurgeryID", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
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
