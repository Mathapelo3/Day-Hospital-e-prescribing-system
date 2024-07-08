using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc;

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
            return View();
        }

        public IActionResult Patients()
        {
            return View();
        }

        public IActionResult AddPatients()
        {
            return View();
        }

        public IActionResult Surgeries()
        {
            return View();
        }

        public IActionResult NewPrescription()
        {
            return View();
        }

        public IActionResult EditPrescription()
        {
            return View();
        }

        public IActionResult NewSurgery()
        {
            return View();
        }

        public IActionResult PatientRecord()
        {
            return View();
        }

        public IActionResult DischargePatient()
        {
            return View();
        }

        public IActionResult ConfirmTreatmentCodes()
        {
            return View();
        }
    }
}
