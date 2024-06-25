using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult DayHospitalRecords()
        {
            var model = _context.HospitalRecords.Include(r => r.Suburb).ThenInclude(s => s.City).ToList();
            var suburbs = _context.Suburbs.Include(s => s.City).ToList();

            ViewBag.Suburbs = new SelectList(suburbs, "SuburbID", "Name");

            return View(model);
        }
       
        private bool HospitalRecordExists(int id)
        {
            return _context.HospitalRecords.Any(e => e.HospitalRecordID == id);
        }

        [HttpPost]
        public IActionResult UpdateHospitalRecord(int id, [FromBody] UpdateHospitalRecordModel model)
        {
            var record = _context.HospitalRecords.Find(id);
            if (record != null)
            {
                if (model.Field == "SuburbID")
                {
                    if (int.TryParse(model.Value, out int suburbId))
                    {
                        var suburb = _context.Suburbs.Include(s => s.City).FirstOrDefault(s => s.SuburbID == suburbId);
                        if (suburb != null)
                        {
                            record.SuburbID = suburb.SuburbID;
                            record.Suburb = suburb;
                        }
                    }
                }
                else
                {
                    // Update other fields
                    _context.Entry(record).Property(model.Field).CurrentValue = model.Value;
                }

                _context.SaveChanges();
                return Json(new { success = true, suburb = record.Suburb });
            }
            return Json(new { success = false });
        }

        public JsonResult GetCityBySuburb(int id)
        {
            var suburb = _context.Suburbs.Include(s => s.City).FirstOrDefault(s => s.SuburbID == id);
            if (suburb != null)
            {
                return Json(new { cityName = suburb.City.Name, postalCode = suburb.PostalCode });
            }
            return Json(new { cityName = "", postalCode = "" });
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
