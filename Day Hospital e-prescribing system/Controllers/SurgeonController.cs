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
        public async Task<ActionResult> Prescriptions(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Prescriptions action called with id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", id);
                return NotFound();
            }

            var prescriptions = await _context.Prescriptions
                .Where(p => p.PatientID == id)
                .Include(p => p.Medication)
                .Select(p => new PatientPrescriptionViewModel
                {
                    PrescriptionID = p.PrescriptionID, 
                    MedicationName = p.Medication.Name,
                    Instruction = p.Instruction,
                    Date = p.Date,
                    Quantity = p.Quantity,
                    Status = p.Status,
                    Urgency = p.Urgency,
                    PatientName = patient.Name 
                })
                .ToListAsync();

            return View(prescriptions);
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
        public IActionResult Surgeries()
        {
            var surgeries = _context.Surgeries.Include(s => s.Patients)
                                              .Include(s => s.Surgery_TreatmentCodes)
                                              .Include(s => s.Theatres)
                                              .Include(s => s.Anaesthesiologists)
                                              .Select(s => new Surgery
                                              {
                                                  SurgeryID = s.SurgeryID,
                                                  Date = s.Date,
                                                  Time = s.Time,
                                                  PatientID = s.PatientID,
                                                  Surgery_TreatmentCodeID = s.Surgery_TreatmentCodeID,
                                                  TheatreID = s.TheatreID,
                                                  AnaesthesiologistID = s.AnaesthesiologistID
                                              })
                                              .ToList();

            var currentDate = DateTime.Today.ToString("yyyy-MM-dd");
            var filteredSurgeries = surgeries.Where(s => s.Date.Date.ToString("yyyy-MM-dd") == currentDate);

            return View(filteredSurgeries);
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
        public IActionResult NewPrescription()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var patientSelectListItems = _context.Patients.Select(p => new SelectListItem
            {
                Value = p.PatientID.ToString(),
                Text = $"{p.Name} {p.Surname}"
            }).ToList();

            var medicationSelectListItems = _context.Medication.Select(m => new SelectListItem
            {
                Value = m.MedicationID.ToString(),
                Text = m.Name
            }).ToList();

            var viewModel = new PrescriptionViewModel
            {
                PatientList = new SelectList(patientSelectListItems, "Value", "Text"),
                MedicationList = new SelectList(medicationSelectListItems, "Value", "Text")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPrescription(PrescriptionViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Starting to capture new prescription...");

            var medications = await _context.Medication.ToListAsync();
            model.MedicationList = new MultiSelectList(medications, "MedicationID", "Name");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid.");
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
            var surgeonIdClaim = User.Claims.FirstOrDefault(c => c.Type == "SurgeonID"); 
            if (surgeonIdClaim == null)
            {
                _logger.LogError("Surgeon ID not found in claims.");
                ModelState.AddModelError("", "Surgeon ID not found in claims");
                return View(model);
            }

            int surgeonId = int.Parse(surgeonIdClaim.Value);
            var surgeon = await _context.Surgeons.FirstOrDefaultAsync(s => s.SurgeonID == surgeonId); 
            if (surgeon == null)
            {
                _logger.LogError("Surgeon not found.");
                ModelState.AddModelError("", "Surgeon not found");
                return View(model);
            }

            var patientId = model.SelectedPatientId; 
            _logger.LogInformation($"PatientID: {patientId}");

            if (model.SelectedMedications != null && model.SelectedMedications.Any())
            {
                foreach (var medication in model.SelectedMedications)
                {
                    var prescription = new Prescription
                    {
                        PatientID = model.SelectedPatientId,
                        MedicationID = medication.MedicationID,
                        Quantity = medication.Quantity,
                        Date = model.Date,
                        Urgency = model.Urgency,
                        SurgeonID = surgeon.SurgeonID,
                        Instruction = medication.Instruction 
                    };

                    _context.Prescriptions.Add(prescription);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"{model.SelectedMedications.Count} prescriptions added to the database.");
            }

            return RedirectToAction("Prescriptions", "Surgeon"); // Adjusted to redirect to a view prescriptions action
        }

        public async Task<IActionResult> EditPrescription(int? id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            var viewModel = new PrescriptionViewModel
            {
                PrescriptionID = prescription.PrescriptionID,
                Instruction = prescription.Instruction,
                Date = prescription.Date,
                Quantity = prescription.Quantity,
                Status = prescription.Status,
                Urgency = prescription.Urgency
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPrescription(int id, PrescriptionViewModel model)
        {
            if (id != model.PrescriptionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = await _context.Prescriptions.FindAsync(id);
                    if (prescription == null)
                    {
                        return NotFound();
                    }

                    prescription.Instruction = model.Instruction;
                    prescription.Date = model.Date;
                    prescription.Quantity = model.Quantity;
                    prescription.Status = model.Status;
                    prescription.Urgency = model.Urgency;

                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Prescription record updated successfully.");
                    return RedirectToAction("Prescriptions", "Surgeon");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the prescription record: {Message}", ex.Message);
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
        public IActionResult NewSurgery()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var anaesthesiologistSelectListItems = _context.Anaesthesiologists.Select(a => new SelectListItem
            {
                Value = a.AnaesthesiologistID.ToString(),
                Text = $"{a.User.Name} {a.User.Surname}"
            }).ToList();

            var patientsSelectListItems = _context.Patients.Select(p => new SelectListItem
            {
                Value = p.PatientID.ToString(),
                Text = $"{p.Name} {p.Surname}"
            }).ToList();

            var theatreSelectListItems = _context.Theatres.Select(n => new SelectListItem
            {
                Value = n.TheatreID.ToString(),
                Text = n.Name
            }).ToList();

            var treatmentCodeSelectListItems = _context.TreatmentCodes.Select(t => new SelectListItem
            {
                Value = t.TreatmentCodeID.ToString(),
                Text = $"{t.ICD_10_Code} - {t.Description}"
            }).ToList();

            var viewModel = new SurgeryViewModel
            {
                AnaesthesiologistList = new SelectList(anaesthesiologistSelectListItems, "Value", "Text"),
                PatientList = new SelectList(patientsSelectListItems, "Value", "Text"),
                TheatreList = new SelectList(theatreSelectListItems, "Value", "Text"),
                TreatmentCodeList = new SelectList(treatmentCodeSelectListItems, "Value", "Text")
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewSurgery(SurgeryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var surgery = new Surgery
                {
                    Date = model.Date,
                    Time = model.Time,
                    TheatreID = model.TheatreID,
                    AnaesthesiologistID = model.AnaesthesiologistID,
                    PatientID = model.PatientID,
                    // Set Surgery_TreatmentCodeID to null initially since it will be set later
                    Surgery_TreatmentCodeID = null
                };

                _context.Surgeries.Add(surgery);
                await _context.SaveChangesAsync(); // Save the surgery record to generate its ID

                var surgeryId = surgery.SurgeryID; // Retrieve the generated ID

                // Create a Surgery_TreatmentCode for each selected treatment code
                foreach (var treatmentCodeId in model.SelectedTreatmentCodes)
                {
                    var treatmentCode = new Surgery_TreatmentCode
                    {
                        Surgery_TreatmentCodeID = surgeryId,
                    };
                    _context.Surgery_TreatmentCodes.Add(treatmentCode);
                }

                await _context.SaveChangesAsync(); // Save the associations

                return RedirectToAction("Surgeries", "Surgeon");
            }

            return View(model);
        }


        public async Task<IActionResult> PatientRecord(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid patient ID.");
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Allergy).ThenInclude(pa => pa.Allergy)
                .Include(p => p.Patient_Vitals).ThenInclude(pv => pv.Vitals)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                return NotFound($"Patient with ID {id} not found.");
            }

            var allergies = patient.Patient_Allergy.Select(pa => new Allergy
            {
                Name = pa.Allergy.Name,
            }).ToList();

            var vitals = patient.Patient_Vitals.Select(pv => new Vitals
            {
                Vital = pv.Vitals.Vital,
                Min = pv.Vitals.Min,
                Max = pv.Vitals.Max,
            }).ToList();

            var viewModel = new PatientRecordViewModel
            {
                Patient = patient,
                Allergies = allergies, 
                Vitals = vitals,
            };

            return View(viewModel);
        }


        public IActionResult DischargePatient()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var items = _context.Patients.Where(p => p.Status == "Discharged").OrderBy(p => p.Name).ToList();
            ViewBag.Patient = items;

            return View();
        }

        public IActionResult ConfirmTreatmentCodes()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            return View();
        }
    }
}
