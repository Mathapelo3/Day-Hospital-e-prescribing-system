using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.ViewModel.PharmacistViewModel;
using Day_Hospital_e_prescribing_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnaesthesiologistController> _logger;

        public PharmacistController(ApplicationDbContext context, ILogger<AnaesthesiologistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public ActionResult PharmacistDashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username"); // Use HttpContext.Session

          
            return View();
        }

        // Medicine
        public IActionResult MedicineList()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        public IActionResult ReceiveMedicine()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var selectListItems = _context.DayHospitalMedications.Select(m => new SelectListItem
            {
                Value = m.StockID.ToString(),
                Text = m.MedicationName
            }).ToList();

            var viewModel = new MedicationViewModel
            {
                DayHospitalMedicationList = new SelectList(selectListItems, "Value", "Text")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveMedicine(MedicationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medicine = new Medication
                    {
                        Name = model.Name,
                        Quantity = model.Quantity
                       
                    };

                    _context.Add(medicine);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Reorder Place Successfully");
                    return RedirectToAction("ReOrder", "Pharmacist");
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

        public IActionResult OrderMedicine()
        {

            return View();
        }

        public async Task<IActionResult> OrderMedicine(AddMedicineViewModel model)
        {
           
            return View();
        }


        public async Task<ActionResult> Prescriptions(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentDate"] = startDate?.ToString("dd-MM-yyyy");
            

            var prescriptionsQuery = from pr in _context.Prescriptions
                                join p in _context.Patients on pr.PatientID equals p.PatientID
                                join s in _context.Surgeons on pr.SurgeonID equals s.SurgeonID
                                join u in _context.Users on s.UserID equals u.UserID
                                join d in _context.DayHospitalMedications on pr.MedicationID equals d.StockID
                                where pr.Status != "Dispensed"
                                select new ViewModel.PrescriptionViewModel
                                {
                                    PrescriptionID = pr.PrescriptionID,
                                    Medication = d.MedicationName,
                                    Instruction = pr.Instruction,
                                    Date = pr.Date,
                                    Quantity = pr.Quantity,
                                    Status = pr.Status,
                                    PatientID = pr.PatientID,
                                    Name = $"{p.Name} {p.Surname}",
                                    Surgeon = $"{u.Username} {u.Surname}",
                                    Urgency = pr.Urgency

                                };

            if (startDate.HasValue)
            {
                prescriptionsQuery = prescriptionsQuery.Where(pa => pa.Date.Date >= startDate.Value.Date);
            }

            var prescriptions = await prescriptionsQuery.ToListAsync();

            return View(prescriptions);
        }

        public async Task<IActionResult> ViewPrescription(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Prescription action called with id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .Include(p => p.Patient_Condition)
                .Include(p => p.Patient_Allergy)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var prescriptions = await _context.Prescriptions
                .Where(p => p.PatientID == id)
                .Include(p => p.Medication)
                .Include(p => p.Surgeon)
                .ThenInclude(s => s.User)
                .ToListAsync();


            

            var viewModel = new ViewModel.PrescriptionViewModel
            {
                Prescriptions = prescriptions.Select(p => new ViewModel.PrescriptionViewModel
                {
                    PrescriptionID = p.PrescriptionID,
                    MedicationName = p.Medication.Name,
                    Instruction = p.Instruction,
                    Date = p.Date,
                    Quantity = p.Quantity,
                    Status = p.Status,
                    Urgency = p.Urgency,
                    Name = patient.Name,
                    Surname = patient.Surname,
                    Patient = $"{patient.Name} {patient.Surname}",
                    //Allergies = p.Allergies,
                    Conditions = p.Conditions,
                    //Vitals = string.Join(", ", p.Vitals.Select(v => v.Value)),
                    //Time = p.Time.ToString(),
                    //Height = p.Height.ToString(),
                    //Temp = p.Temperature.ToString(),
                    //Max = p.MaxValue.ToString(),
                    //Min = p.MinValue.ToString(),
                    //Vital = p.VitalSigns.ToString(),
                    //ChronicMedication = p.ChronicMedication,
                    Surgeon = p.Surgeon.User.Name + " " + p.Surgeon.User.Surname,
                    PatientID = p.PatientID,
                    //SurgeryID = p.SurgeryID,
                    MedicationID = p.MedicationID
                    
                    
                }).ToList()
            };

            return View(viewModel);
        }


        public IActionResult RejectPrescription()
        {
            return PartialView("_RejectPrescriptionPartialView");
        }

        public async Task<IActionResult> ListDispensedPrescriptions(string searchString, DateTime? Date)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDate"] = Date?.ToString("yyyy-MM-dd");

            var dispensedPrescriptionQuery = from d in _context.Prescriptions
                                             join p in _context.Patients on d.PatientID equals p.PatientID
                                             where d.Status != "Not Dispensed"
                                             select new DispensedPrescription
                                             {
                                                 PatientID = d.PatientID,
                                                 Patient = $"{p.Name} {p.Surname}",
                                                 Date = d.Date
                                             };

            if (!String.IsNullOrEmpty(searchString))
            {
                dispensedPrescriptionQuery = dispensedPrescriptionQuery.Where(dp => dp.Patient.ToLower().Contains(searchString.ToLower()));
            }

            if (Date.HasValue)
            {
                dispensedPrescriptionQuery = dispensedPrescriptionQuery.Where(dp => dp.Date.Date == Date.Value.Date);
            }

            var dispensedPrescriptions = await dispensedPrescriptionQuery.ToListAsync();

            return View(dispensedPrescriptions);
        }
        public IActionResult OrderedMedicine()
        {
            return View();
        }

        public IActionResult NewStock()
        {
            return View();
        }

        public async Task<ActionResult> PatientPrescriptions(int id)
        {

            return View();
        }

        public IActionResult Admission()
        {
            return View();
        }

        public IActionResult MedicineLogs()
        {
            return View();
        }

    }
}
