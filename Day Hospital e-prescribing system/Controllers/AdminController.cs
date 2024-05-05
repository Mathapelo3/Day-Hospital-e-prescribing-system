using Microsoft.AspNetCore.Mvc;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult DayHospitalRecords()
        {
            return View();
        }
        public IActionResult MedicalProfessionals()
        {
            return View();
        }

    }
}
