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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatients(AddPatientsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var patient = new Patient
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Gender = model.Gender,
                        Email = model.Email,
                        IDNo = model.IDNo,
                        Status = model.Status,
                    };

                    _context.Add(patient);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Patient added successfully.");
                    return RedirectToAction("Patients", "Surgeon");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while adding patient: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
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
        public async Task<IActionResult> AllPrescriptions(DateTime? filterDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var prescriptions = _context.Prescriptions
                            .Include(p => p.Patient)
                            .Include(p => p.Surgeon)
                            .ThenInclude(s => s.User)
                            .Include(p => p.Medication)
                            .Select(p => new AllPrescriptionsViewModel()
                            {
                                PrescriptionID = p.PrescriptionID,
                                PatientID = p.PatientID,
                                SurgeonID = p.SurgeonID,
                                MedicationID = p.MedicationID,
                                PatientName = p.Patient.Name,
                                PatientSurname = p.Patient.Surname,
                                SurgeonName = p.Surgeon.User.Name,
                                SurgeonSurname = p.Surgeon.User.Surname,
                                MedicationName = p.Medication.Name,
                                Date = p.Date,
                                Instruction = p.Instruction,
                                Quantity = p.Quantity,
                                Status = p.Status,
                                Urgency = p.Urgency
                            })
                             .ToList();

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
                MedicationList = _context.Medication
                    .Where(m => m.Name != null)
                    .Select(m => new SelectListItem
                    {
                        Value = m.MedicationID.ToString(),
                        Text = m.Name
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
            model.MedicationList = _context.Medication
                .Where(m => m.Name != null)
                .Select(m => new SelectListItem
                {
                    Value = m.MedicationID.ToString(),
                    Text = m.Name
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


        [HttpGet]
        public IActionResult NewSurgery()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var viewModel = new SurgeryViewModel
            {
                AnaesthesiologistList = _context.Anaesthesiologists
                    .Select(a => new SelectListItem
                    {
                        Value = a.AnaesthesiologistID.ToString(),
                        Text = $"{a.User.Name} {a.User.Surname}"
                    }).ToList(),
                PatientList = _context.Patients
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname}"
                    }).ToList(),
                TheatreList = _context.Theatres
                    .Select(n => new SelectListItem
                    {
                        Value = n.TheatreID.ToString(),
                        Text = n.Name
                    }).ToList(),
                TreatmentCodeList = _context.TreatmentCodes
                    .Select(t => new SelectListItem
                    {
                        Value = t.TreatmentCodeID.ToString(),
                        Text = $"{t.ICD_10_Code} - {t.Description}"
                    }).ToList(),

                SurgeonList = _context.Surgeons
                    .Select(s => new SelectListItem
                    {
                        Value = s.SurgeonID.ToString(),
                        Text = $"{s.User.Name} {s.User.Surname}"
                    }).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewSurgery(SurgeryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var newSurgery = new Surgery
                    {
                        Date = model.Date,
                        Time = model.Time,
                        PatientID = model.PatientID,
                        SurgeonID = model.SurgeonID,
                        AnaesthesiologistID = model.AnaesthesiologistID,
                        TheatreID = model.TheatreID,
                    };
                    _context.Surgeries.Add(newSurgery);
                    await _context.SaveChangesAsync();

                    // Add selected treatment codes
                    if (model.SelectedTreatmentCodes != null && model.SelectedTreatmentCodes.Any())
                    {
                        foreach (var treatmentCodeId in model.SelectedTreatmentCodes)
                        {
                            var surgeryTreatmentCode = new Surgery_TreatmentCode
                            {
                                SurgeryID = newSurgery.SurgeryID,
                                TreatmentCodeID = treatmentCodeId
                            };
                            _context.Surgery_TreatmentCodes.Add(surgeryTreatmentCode);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Surgeries));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, "Unable to save changes. " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            // Repopulate the dropdown lists
            model.AnaesthesiologistList = _context.Anaesthesiologists.Select(a => new SelectListItem
            {
                Value = a.AnaesthesiologistID.ToString(),
                Text = $"{a.User.Name} {a.User.Surname}"
            }).ToList();
            model.PatientList = _context.Patients.Select(p => new SelectListItem
            {
                Value = p.PatientID.ToString(),
                Text = $"{p.Name} {p.Surname}"
            }).ToList();
            model.SurgeonList = _context.Surgeons.Select(s => new SelectListItem
            {
                Value = s.SurgeonID.ToString(),
                Text = $"{s.User.Name} {s.User.Surname}"
            }).ToList();
            model.TheatreList = _context.Theatres.Select(n => new SelectListItem
            {
                Value = n.TheatreID.ToString(),
                Text = n.Name
            }).ToList();
            model.TreatmentCodeList = _context.TreatmentCodes.Select(t => new SelectListItem
            {
                Value = t.TreatmentCodeID.ToString(),
                Text = $"{t.ICD_10_Code} - {t.Description}"
            }).ToList();

            return View(model);
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
