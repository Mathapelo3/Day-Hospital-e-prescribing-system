using Microsoft.AspNetCore.Mvc;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AnaesthesiologistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult AnaesthesiologistDashboard()
        {
            string username = HttpContext.Session.GetString("UserName"); // Use HttpContext.Session

            // Pass the username to the view
            ViewBag.Username = username;
            return View();
        }
        public ActionResult ViewPatients()
        {
            return View();
        }
        public ActionResult AdmissionRecords()
        {
            return View();
        }
        public ActionResult MedicalHistory()
        {
            return View();
        }
        public ActionResult Prescriptions()
        {
            return View();
        }
        public ActionResult Orders()
        {
            return View();
        }
        public ActionResult CaptureOrders()
        {
            return View();
        }
        public ActionResult ViewOrders()
        {
            return View();
        }
        public ActionResult EditOrders()
        {
            return View();
        }
        public ActionResult MedicationRecords()
        {
            return View();
        }
        public ActionResult MaintainVitals()
        {
            return View();
        }
    }
}
