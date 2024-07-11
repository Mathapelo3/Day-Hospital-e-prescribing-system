using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Day_Hospital_e_prescribing_system.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Day_Hospital_e_prescribing_system.ViewModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Data.SqlTypes;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class AnaesthesiologistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnaesthesiologistController> _logger;

        public AnaesthesiologistController(ApplicationDbContext context, ILogger<AnaesthesiologistController> logger)
        {
            _context = context;
            _logger = logger;

        }
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
        public async Task<ActionResult> ViewPatients(string searchString,  DateTime? Date)
        {
            ViewData["CurrentFilter"] = searchString;
            
            ViewData["CurrentDate"] = Date?.ToString("yyyy-MM-dd");

            var patientAdmissions = from p in _context.Patients
                                    join a in _context.Admissions on p.PatientID equals a.PatientID
                                    join w in _context.Wards on p.WardID equals w.WardID
                                    join n in _context.Nurses on a.NurseID equals n.NurseID
                                    join u in _context.Users on n.UserID equals u.UserID
                                    select new PatientViewModel
                                    {
                                        PatientID = p.PatientID,
                                        Patient = p.Name + " " + p.Surname,
                                        Date = a.Date,
                                        Time = a.Time,
                                        Ward = w.Name,
                                        Bed = w.Bed,  // Assuming Bed is a property in the Ward table
                                        Nurse = u.Name + " " + u.Surname,  // Assuming Name is a property in the Nurse table
                                        Status = p.Status
                                    };

            if (!String.IsNullOrEmpty(searchString))
            {
                patientAdmissions = patientAdmissions.Where(pa => pa.Patient.Contains(searchString));
            }


            if (Date.HasValue)
            {
                patientAdmissions = patientAdmissions.Where(pa => pa.Date.Date == Date.Value.Date);
            }

            

            return View(await patientAdmissions.ToListAsync());
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
        public IActionResult MaintainVitals()
        {
            var vitals = _context.Vitals.Select(v => new VitalsViewModel
            {
                VitalsID = v.VitalsID,
                Vital = v.Vital,
                Min = v.Min,
                Max = v.Max
            }).ToList();

            // Debugging output
            //ViewBag.DebugMessage = "Vitals Count: " + vitals.Count;
            //foreach (var vital in vitals)
            //{
            //    ViewBag.DebugMessage += $" | VitalsID: {vital.VitalsID}, Vital: {vital.Vital}, Min: {vital.Min}, Max: {vital.Max}";
            //}
            return View(vitals);
        }

        public async Task<IActionResult> EditVital(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitals = await _context.Vitals.FindAsync(id);
            if (vitals == null)
            {
                return NotFound();
            }

            var viewModel = new VitalsViewModel
            {
                VitalsID = vitals.VitalsID,
                Vital = vitals.Vital,
                Min = vitals.Min,
                Max = vitals.Max
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVital(int id, VitalsViewModel model)
        {
            if (id != model.VitalsID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var vitals = await _context.Vitals.FindAsync(id);
                    if (vitals == null)
                    {
                        return NotFound();
                    }

                    vitals.Vital = model.Vital;
                    vitals.Min = model.Min;
                    vitals.Max = model.Max;

                    _context.Update(vitals);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Vital record updated successfully.");
                    return RedirectToAction("MaintainVitals", "Anaesthesiologist");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the vital record: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVital(int id)
        {
            try
            {
                var vitals = await _context.Vitals.FindAsync(id);
                if (vitals == null)
                {
                    return NotFound();
                }

                _context.Vitals.Remove(vitals);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vital record deleted successfully.");
            }
            catch (SqlNullValueException ex)
            {
                _logger.LogError(ex, "Null value encountered: {Message}", ex.Message);
                ModelState.AddModelError("", "Null value encountered. Ensure all required fields are filled.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the vital record: {Message}", ex.Message);
                ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction(nameof(MaintainVitals));
        }

        public IActionResult AddVitals()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVitals(VitalsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vitals = new Vitals
                    {
                        Vital = model.Vital,
                        Min = model.Min,
                        Max = model.Max
                    };

                    _context.Add(vitals);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Vital record created successfully.");
                    return RedirectToAction("MaintainVitals", "Anaesthesiologist");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the vital record: {Message}", ex.Message);
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            return View(model);
        }
    }
}
