using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AdminDashboard()
        {
            return View();
        }

        public async Task<IActionResult>  DayHospitalRecords()
        {
            var hospitalRecords = await _context.HospitalRecords.ToListAsync();
            return View(hospitalRecords);
        }
        public IActionResult EditHospitalRecords()
        {
            return View();
        }
        public IActionResult MedicalProfessionals()
        {
            return View();
        }
        public IActionResult AddMedicalProfessional()
        {
            return View();
        }
        public IActionResult TheatreRecords()
        {
            return View();
        }
        public IActionResult WardRecords()
        {
            return View();
        }
        public IActionResult ChronicConditionRecords()
        {
            return View();
        }
        public IActionResult AddChronicCondition()
        {
            return View();
        }
        public IActionResult EditChronicCondition()
        {
            return View();
        }
        public IActionResult AddContraIndication()
        {
            return View();
        }
        public IActionResult ContraIndicationRecords()
        {
            return View();
        }
        public IActionResult AddMedicationInteraction()
        {
            return View();
        }
        public IActionResult MedicationInteractionRecords()
        {
            return View();
        }
    }
}
