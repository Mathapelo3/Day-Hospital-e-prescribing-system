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
using System.Security.Claims;
using Day_Hospital_e_prescribing_system.ViewModels;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class SurgeonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SurgeonController> _logger;
        private readonly IConfiguration _config;
        public SurgeonController(ApplicationDbContext context, ILogger<SurgeonController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
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
        public async Task<IActionResult> Surgeries(DateTime? filterDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var query = _context.Surgeries
                .Include(s => s.Patients)
                .Include(s => s.Theatres)
                .Include(s => s.Anaesthesiologists)
                    .ThenInclude(a => a.User)
                .Include(s => s.Surgery_TreatmentCodes)
                    .ThenInclude(stc => stc.TreatmentCodes)
                .AsQueryable();

            if (filterDate.HasValue)
            {
                query = query.Where(s => s.Date.Date == filterDate.Value.Date);
            }

            var surgeries = await query
                .Select(s => new SurgeryDetailsViewModel
                {
                    SurgeryID = s.SurgeryID,
                    PatientID = s.PatientID,
                    AnaesthesiologistID = s.AnaesthesiologistID,
                    TheatreID = s.TheatreID,
                    PatientName = s.Patients.Name,
                    PatientSurname = s.Patients.Surname,
                    TheatreName = s.Theatres.Name,
                    AnaesthesiologistName = s.Anaesthesiologists.User.Name,
                    AnaesthesiologistSurname = s.Anaesthesiologists.User.Surname,
                    Date = s.Date,
                    Time = s.Time,
                    TreatmentCodes = s.Surgery_TreatmentCodes.Select(stc => stc.ICD_10_Code).ToList()
                })
                .ToListAsync();

            ViewBag.FilterDate = filterDate;
            return View(surgeries);
        }


        [HttpGet]
        public IActionResult GetPatients()
        {
            var selectListItems = _context.Patients.Select(p => new SelectListItem
            {
                Value = p.PatientID.ToString(),
                Text = $"{p.Name} {p.Surname}"
            }).ToList();

            var viewModel = new PrescriptionViewModel
            {
                PatientList = new SelectList(selectListItems, "Value", "Text")
            };

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult GetMedication()
        {
            var selectListItems = _context.Medication.Select(p => new SelectListItem
            {
                Value = p.MedicationID.ToString(),
                Text = p.Name
            }).ToList();

            var viewModel = new PrescriptionViewModel
            {
                MedicationList = new SelectList(selectListItems, "Value", "Text")
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> NewPrescription()
        {
            var model = new PrescriptionViewModel
            {
                Date = DateTime.Today
            };
            await PopulateDropdownLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPrescription(PrescriptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid: " + string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)));
                await PopulateDropdownLists(model);
                return View(model);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("User ID claim not found");
                ModelState.AddModelError("", "User ID not found in claims");
                await PopulateDropdownLists(model);
                return View(model);
            }
            int userId = int.Parse(userIdClaim.Value);

            var surgeonIdClaim = User.Claims.FirstOrDefault(c => c.Type == "SurgeonID");
            if (surgeonIdClaim == null)
            {
                _logger.LogError("Surgeon ID claim not found");
                ModelState.AddModelError("", "Surgeon ID not found in claims");
                await PopulateDropdownLists(model);
                return View(model);
            }
            int surgeonId = int.Parse(surgeonIdClaim.Value);

            var surgeon = await _context.Surgeons.FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);
            if (surgeon == null)
            {
                _logger.LogError($"Surgeon not found for ID: {surgeonId}");
                ModelState.AddModelError("", "Surgeon not found");
                await PopulateDropdownLists(model);
                return View(model);
            }

            if (model.SelectedMedications != null && model.SelectedMedications.Any())
            {
                try
                {
                    foreach (var medication in model.SelectedMedications)
                    {
                        if (medication == null)
                        {
                            _logger.LogWarning("Null medication encountered in SelectedMedications");
                            continue;
                        }

                        var prescription = new Prescription
                        {
                            PatientID = model.SelectedPatientId,
                            MedicationID = medication.StockID,
                            Quantity = medication.Quantity ?? "0",
                            Date = model.Date,
                            Urgency = model.Urgency,
                            SurgeonID = surgeon.SurgeonID,
                            Instruction = medication.Instruction ?? "",
                        };
                        _context.Prescriptions.Add(prescription);
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"{model.SelectedMedications.Count} prescriptions added to the database.");
                    return RedirectToAction("Prescriptions", new { patientId = model.SelectedPatientId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving prescriptions: {ex.Message}");
                    ModelState.AddModelError("", "Error saving prescriptions. Please try again.");
                    await PopulateDropdownLists(model);
                    return View(model);
                }
            }
            else
            {
                _logger.LogWarning("No medications selected");
                ModelState.AddModelError("", "No medications selected");
                await PopulateDropdownLists(model);
                return View(model);
            }
        }

        private async Task PopulateDropdownLists(PrescriptionViewModel model)
        {
            try
            {
                var patients = await _context.Patients
                    .Where(p => p.Name != null && p.Surname != null)
                    .Select(p => new SelectListItem
                    {
                        Value = p.PatientID.ToString(),
                        Text = $"{p.Name} {p.Surname} ({p.IDNo})"
                    })
                    .ToListAsync();

                if (!patients.Any())
                {
                    _logger.LogWarning("No patients found in the database");
                }

                model.PatientList = new SelectList(patients, "Value", "Text");

                var medications = await _context.DayHospitalMedications
                    .Where(m => m.MedicationName != null)
                    .Select(m => new SelectListItem
                    {
                        Value = m.StockID.ToString(),
                        Text = m.MedicationName
                    })
                    .ToListAsync();

                if (!medications.Any())
                {
                    _logger.LogWarning("No medications found in the database");
                }

                model.MedicationList = new MultiSelectList(medications, "Value", "Text");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error populating dropdown lists: {ex.Message}");
                throw;
            }
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
            //ViewBag.Username = HttpContext.Session.GetString("Username");

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
                                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? null : reader.GetString(reader.GetOrdinal("DateOfBirth")),
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


        //public async Task<IActionResult> PatientRecord(int id)
        //{
        //    if (id <= 0)
        //    {
        //        return BadRequest("Invalid patient ID.");
        //    }

        //    try
        //    {
        //        var patient = await _context.Patients
        //            .Include(p => p.Patient_Allergy)
        //                .ThenInclude(pa => pa.Allergy)
        //            .Include(p => p.Patient_Vitals)
        //                .ThenInclude(pv => pv.Vitals)
        //            .FirstOrDefaultAsync(p => p.PatientID == id);

        //        if (patient == null)
        //        {
        //            return NotFound($"Patient with ID {id} not found.");
        //        }

        //        // Now that we have the data, we can select the properties we need
        //        var allergies = patient.Patient_Allergy
        //            .Select(pa => new Allergy
        //            {
        //                Name = pa.Allergy.Name,
        //            })
        //            .ToList();

        //        var vitals = patient.Patient_Vitals
        //            .Where(pv => pv.Vitals != null)
        //            .Select(pv => new Vitals
        //            {
        //                Vital = pv.Vitals.Vital ?? "Unknown",
        //                Min = pv.Vitals.Min ?? "Unknown",
        //                Max = pv.Vitals.Max ?? "Unknown",
        //            })
        //            .ToList();

        //        var viewModel = new PatientRecordViewModel
        //        {
        //            Patient = patient,
        //            Allergies = allergies,
        //            Vitals = vitals,
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception if necessary
        //        return StatusCode(500, $"An error occurred while processing your request: {ex.Message}");
        //    }
        //}


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

        //public IActionResult ConfirmTreatmentCodes()
        //{
        //    ViewBag.Username = HttpContext.Session.GetString("Username");

        //    return View();
        //}

    }
}
