using Microsoft.AspNetCore.Mvc;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class SurgeonController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
