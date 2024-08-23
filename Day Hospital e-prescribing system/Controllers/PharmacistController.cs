using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.ViewModel.PharmacistViewModel;
using Day_Hospital_e_prescribing_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }
        public IActionResult ReceiveMedicine()
        {
            return View();
        }

        public IActionResult OrderMedicine()
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
                                join d in _context.DayHospitalMedication on pr.MedicationID equals d.StockID
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
                                    Surgeon = $"{u.Username} {u.Surname}"


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

            var patient = await _context.Patients.Include(p => p.Patient_Vitals).Include(p => p.Patient_Condition).Include(p => p.Patient_Allergy).FirstOrDefaultAsync(p => p.PatientID == id);
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

            var vitals = patient.Patient_Vitals;
            var conditions = patient.Patient_Condition;
            var allergies = patient.Patient_Allergy;

            var viewModel = new ViewModel.PharmacistViewModel.PrescriptionViewModel
            {
                prescriptions = prescriptions.Select(p => new ViewModel.PharmacistViewModel.PrescriptionViewModel.PrescriptionViewModel
                {
                    PrescriptionID = p.PrescriptionID,
                    Medication = p.Medication.Name,
                    Instruction = p.Instruction,
                    Date = p.Date,
                    Quantity = p.Quantity,
                    Status = p.Status,
                    Urgency = p.Urgency,
                    Name = patient.Name,
                    Surname = patient.Surname,
                    PatientID = p.PatientID,
                    SurgeryID = p.SurgeonID,
                    MedicationID = p.MedicationID,
                    Surgeon = p.Surgeon.User.Name + " " + p.Surgeon.User.Surname,
                    Time = p.Time.ToString(), // Assuming 'Time' is a property of Prescription
                    Notes = p.Notes,
                    Height = p.Height.ToString(), // Assuming 'Height' is a property of Prescription
                    Weight = p.Weight.ToString(), // Assuming 'Weight' is a property of Prescription
                    Vitals = vitals.Select(v => new ViewModel.PharmacistViewModel.PrescriptionViewModel.VitalsViewModel
                    {
                        Date = v.Date,
                        Time = v.Time.ToString(),
                        Notes = v.Notes,
                        Height = v.Height.ToString(),
                        Weight = v.Weight.ToString(),
                        Min = v.Min.ToString(),
                        Max = v.Max.ToString(),
                        Value = v.Value.ToString()
                    }).ToList(),
                    Patient_AllergyID = allergies.FirstOrDefault()?.AllergyID ?? -1, // Example handling for allergies
                    Patient_ConditionID = conditions.FirstOrDefault()?.Patient_ConditionID ?? -1 // Example handling for conditions
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
