using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class SurgeonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SurgeonController> _logger;
        public SurgeonController(ApplicationDbContext context, ILogger<SurgeonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ActionResult Dashboard()
        {
            string username = HttpContext.Session.GetString("UserName"); // Use HttpContext.Session

            // Pass the username to the view
            ViewBag.Username = username;
            return View();
        }

        public IActionResult Prescriptions()
        {
            var prescriptions = _context.Prescriptions.ToList();
            ViewBag.Prescription = prescriptions;

            return View();
        }

        public IActionResult Patients()
        {
            var patients = _context.Patients.ToList();
            ViewBag.Patient = patients;

            return View();
        }

        public IActionResult AddPatients()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatients(PatientViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var patient = new Patient
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        IDNo = model.IDNo,
                        ContactNo = model.ContactNo,
                        Status = "Booked"
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

        public IActionResult Surgeries()
        {
            var surgery = _context.Surgeries.ToList();
            ViewBag.Surgery = surgery;

            return View();
        }

        public IActionResult NewPrescription()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPrescription(PrescriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = new Prescription
                    {
                        Instruction = model.Instruction,
                        Date = model.Date,
                        Quantity = model.Quantity,
                        Urgency = model.Urgency
                    };

                    _context.Add(prescription);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Prescription added successfully.");
                    return RedirectToAction("Prescriptions", "Surgeon");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while adding prescription: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }

        public async Task<IActionResult> EditPrescription(int? id)
        {
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

        public IActionResult NewSurgery()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewSurgery(SurgeryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var surgery = new Surgery
                    {
                        Date = model.Date,
                        Time = model.Time,
                        Urgency = model.Urgency,
                        Administered = model.Administered,
                        QAdministered = model.QAdministered,
                        Notes = model.Notes
                    };

                    _context.Add(surgery);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Surgery added successfully.");
                    return RedirectToAction("Surgeries", "Surgeon");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while adding surgery: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }

        public IActionResult PatientRecord()
        {
            return View();
        }

        public IActionResult DischargePatient()
        {
            var items = _context.Patients.Where(p => p.Status == "Discharged").OrderBy(p => p.Name).ToList();
            ViewBag.Patient = items;

            return View();
        }

        public IActionResult ConfirmTreatmentCodes()
        {
            return View();
        }
    }
}
