using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.ViewModel.PharmacistViewModel;
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
                                select new PrescriptionViewModel
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

        public IActionResult ViewPrescription()
        {
            return View();
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

        public IActionResult PatientPrescriptions()
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
