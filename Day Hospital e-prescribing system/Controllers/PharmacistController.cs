using Day_Hospital_e_prescribing_system.Models;
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
            return View();
        }
        public ActionResult PharmacistDashboard()
        {
            string username = HttpContext.Session.GetString("UserName"); // Use HttpContext.Session

            // Pass the username to the view
            ViewBag.Username = username;
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

        public IActionResult Prescriptions()
        {
            return View();
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
            ViewData["CurrentFilter"] = searchString;

            ViewData["CurrentDate"] = Date?.ToString("yyyy-MM-dd");

            var dispensedPrescription = from d in _context.Prescriptions
                                        join p in _context.Patients on d.PatientID equals p.PatientID
                                        select new DispensedPrescription
                                        {
                                            PatientID = d.PatientID,
                                            Patient = p.Name + " " + p.Surname,
                                            Date = d.Date

                                        };

            if (!String.IsNullOrEmpty(searchString))
            {
                dispensedPrescription = dispensedPrescription.Where(dp => dp.Patient.Contains(searchString));
            }


            if (Date.HasValue)
            {
                dispensedPrescription = dispensedPrescription.Where(dp => dp.Date.Date == Date.Value.Date);
            }

            return View( await dispensedPrescription.ToListAsync());
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
