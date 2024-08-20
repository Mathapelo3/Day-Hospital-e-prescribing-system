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
using Day_Hospital_e_prescribing_system.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;


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
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }

        public ActionResult AnaesthesiologistDashboard()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");  // Use HttpContext.Session

            
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> ViewPatients(string searchString, DateTime? Date)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDate"] = Date?.ToString("yyyy-MM-dd");
            _logger.LogInformation("Received date: {Date}", Date?.ToString("yyyy-MM-dd"));
            var patientAdmission = from p in _context.Patients
                                   join a in _context.Admissions on p.PatientID equals a.PatientID
                                   join w in _context.Ward on p.WardId equals w.WardId
                                   join b in _context.Bed on p.WardId equals b.BedId
                                   join n in _context.Nurses on a.NurseID equals n.NurseID
                                   join u in _context.Users on n.UserID equals u.UserID
                                   select new PatientViewModel
                                   {
                                       PatientID = p.PatientID,
                                       Patient = p.Name + " " + p.Surname,
                                       Date = a.Date,
                                       Time = a.Time,
                                       Ward = w.WardName,
                                       Bed = b.BedName, // Assuming Bed is a property in the Ward table
                                       Nurse = u.Name + " " + u.Surname,
                                       Status = p.Status
                                   };

            if (!String.IsNullOrEmpty(searchString))
            {
                patientAdmission = patientAdmission.Where(pa => pa.Patient.Contains(searchString));
            }

            if (Date.HasValue)
            {
                _logger.LogInformation("Filtering by date: {Date}", Date.Value.ToString("yyyy-MM-dd"));
                patientAdmission = patientAdmission.Where(pa => pa.Date.Date == Date.Value.Date);
            }
        

            return View(await patientAdmission.ToListAsync());

            
        }

        public async Task<ActionResult> BookedPatients(string searchString, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("dd-MM-yyyy");
            ViewData["EndDate"] = endDate?.ToString("dd-MM-yyyy");

            var bookedPatients = from s in _context.Surgeries
                                 join p in _context.Patients on s.PatientID equals p.PatientID
                                 join n in _context.Nurses on s.NurseID equals n.NurseID
                                 join su in _context.Surgeons on s.SurgeonID equals su.SurgeonID
                                 join t in _context.Theatres on s.TheatreID equals t.TheatreID
                                 join w in _context.Ward on p.WardId equals w.WardId
                                 join c in _context.Surgery_TreatmentCodes on s.Surgery_TreatmentCodeID equals c.Surgery_TreatmentCodeID
                                 join u in _context.Users on n.UserID equals u.UserID
                                 join us in _context.Users on su.UserID equals us.UserID
                                 select new BookedPatientsViewModel
                                 {
                                     SurgeryID = s.SurgeryID,
                                     PatientID = p.PatientID,
                                     Patient = p.Name + " " + p.Surname,
                                     Date = s.Date,
                                     Time = s.Time,
                                     Name = w.Name,
                                     Bed = w.Bed,  // Assuming Bed is a property in the Ward table 
                                     Nurse = u.Name + " " + u.Surname,
                                     Theatre = t.Name,
                                     Surgeon = u.Name + " " + u.Surname
                                 };

            if (!String.IsNullOrEmpty(searchString))
            {
                bookedPatients = bookedPatients.Where(pa => pa.Patient.Contains(searchString));
            }

            if (startDate.HasValue)
            {
                bookedPatients = bookedPatients.Where(pa => pa.Date.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                bookedPatients = bookedPatients.Where(pa => pa.Date.Date <= endDate.Value.Date);
            }

            return View(await bookedPatients.ToListAsync());
        }
        public async Task<ActionResult> MedicalHistory(int id )
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("MedicalHistory action called with id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Patient_Vitals)
                .ThenInclude(pv => pv.Vitals)
                .Include(p => p.Patient_Allergy)
                .ThenInclude(pa => pa.Allergy)
                .Include(p => p.Patient_Condition)
                .ThenInclude(pc => pc.Condition)
                .Include(p => p.Patient_Medication)
                .ThenInclude(pm => pm.General_Medication)
                .FirstOrDefaultAsync(p => p.PatientID == id);

            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", id);
                return NotFound();
            }

            var model = new PatientViewModel
            {
                PatientID = patient.PatientID,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = patient.Patient_Vitals.FirstOrDefault()?.Date?? DateTime.Now,
                Height = patient.Patient_Vitals.FirstOrDefault()?.Height, // Correct reference
                Weight = patient.Patient_Vitals.FirstOrDefault()?.Weight, // Correct reference
                Vitals = patient.Patient_Vitals.Select(pv => new VitalsViewModel
                {
                    Vital = pv.Vitals.Vital,
                    Min = pv.Vitals.Min,
                    Max = pv.Vitals.Max,
                    Date = pv.Date,
                    Time = pv.Time.ToString(@"hh\:mm"),
                    Notes = pv.Notes
                }).ToList(),
                Allergies = patient.Patient_Allergy.Select(pa => pa.Allergy.Name).ToList(),
                Conditions = patient.Patient_Condition.Select(pc => pc.Condition.Name).ToList(),
                Medications = patient.Patient_Medication.Select(pm => pm.General_Medication.Name).ToList()
            };

            _logger.LogInformation("MedicalHistory action completed successfully for id: {Id}", id);
            return View(model);
        }
        public async Task<ActionResult> Prescriptions(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Prescriptions action called with id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", id);
                return NotFound();
            }

            var prescriptions = await _context.Prescriptions
        .Where(p => p.PatientID == id)
        .Include(p => p.Medication)
        .Include(p => p.Surgeon)
        .ThenInclude(s => s.User)
        .ToListAsync();

            var prescriptionViewModels = prescriptions.Select(p => new PrescriptionViewModel
            {
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
                Surgeon = p.Surgeon.User.Name + " " + p.Surgeon.User.Surname // Assuming you have a Name property in Surgeon model
            }).ToList();

            return View(prescriptionViewModels);
        }
        public async Task<ActionResult> Orders(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Orders action called with id: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid patient id: {Id}", id);
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found with id: {Id}", id);
                return NotFound();
            }

            var orders = await _context.Orders
                .Where(o => o.PatientID == id)
                .Include(o => o.Medication)
                .ToListAsync();

            var orderViewModels = orders.Select(o => new OrderViewModel
            {
                
                Date = o.Date,
                Medication = o.Medication?.Name ?? "No Medication",
                Quantity = o.Quantity,
                Status = o.Status,
                Name = patient.Name,
                Surname = patient.Surname,
                MedicationID = o.MedicationID,
                PatientID = o.PatientID,

            }).ToList();

            return View(orderViewModels);
        }
        public async Task<ActionResult> CaptureOrders(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found with ID: {id}", id);
                return NotFound();
            }

            var medications = await _context.Medication.ToListAsync();
            var model = new OrderFormViewModel
            {
                PatientID = id,
                Name = patient.Name,
                Surname = patient.Surname,
                Date = DateTime.Now,
                Medications = new MultiSelectList(medications, "MedicationID", "Name"),
                SelectedMedications = new List<MedicationViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureOrders(OrderFormViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Starting to capture orders...");

            var medications = await _context.Medication.ToListAsync();
            model.Medications = new MultiSelectList(medications, "MedicationID", "Name");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is not valid.");
                return View(model);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogError("User ID not found in claims.");
                ModelState.AddModelError("", "User ID not found in claims");
                return View(model);
            }

            int userId = int.Parse(userIdClaim.Value);
            var anaesthesiologistIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AnaesthesiologistID");
            if (anaesthesiologistIdClaim == null)
            {
                _logger.LogError("Anaesthesiologist ID not found in claims.");
                ModelState.AddModelError("", "Anaesthesiologist ID not found in claims");
                return View(model);
            }

            int anaesthesiologistId = int.Parse(anaesthesiologistIdClaim.Value);
            var anaesthesiologist = await _context.Anaesthesiologists.FirstOrDefaultAsync(a => a.AnaesthesiologistID == anaesthesiologistId);
            if (anaesthesiologist == null)
            {
                _logger.LogError("Anaesthesiologist not found.");
                ModelState.AddModelError("", "Anaesthesiologist not found");
                return View(model);
            }

            var patientId = model.PatientID; // Check the value of model.PatientID
            _logger.LogInformation($"PatientID: {patientId}");

            if (model.SelectedMedications != null && model.SelectedMedications.Count > 0)
            {
                foreach (var item in model.SelectedMedications)
                {
                    var order = new Orders
                    {
                        PatientID = model.PatientID,
                        MedicationID = item.MedicationID,
                        Quantity = item.Quantity,
                        Date = model.Date,
                        Status = "ordered",
                        Urgency = model.Urgency,
                        AnaesthesiologistID = anaesthesiologist.AnaesthesiologistID
                    };

                    _context.Orders.Add(order);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"{model.SelectedMedications.Count} orders added to the database.");
            }

            return RedirectToAction("ViewOrders", "Anaesthesiologist");
        }


        public async Task<ActionResult> ViewOrders(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var query = _context.Orders
                .Include(o => o.Patient)
                .Include(o => o.Medication)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.Date >= startDate.Value);
                ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.Date <= endDate.Value);
                ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            }

            var orders = await query
                .Select(o => new OrderViewModel
                {
                    OrderID = o.OrderID,
                    Date = o.Date,
                    Patient = o.Patient.Name + " " + o.Patient.Surname,
                    Medication = o.Medication.Name,
                    Quantity = o.Quantity,
                    Status = o.Status
                })
                .ToListAsync();

            return View(orders);
        }
        public async Task<ActionResult> EditOrders(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Patient)
                .Include(o => o.Medication)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null || order.Status != "ordered")
            {
                return NotFound();
            }

            var orderViewModel = new OrderViewModel
            {
                OrderID = order.OrderID,
                Date = order.Date,
                Name = order.Patient.Name,
                Surname = order.Patient.Surname,
                Medication = order.Medication.Name,
                Quantity = order.Quantity,
                Status = order.Status
            };

            var orderEditViewModel = new OrderEditViewModel
            {
                OrderID = order.OrderID,
                Date = order.Date,
                Quantity = order.Quantity,
                Status = order.Status
            };

            ViewBag.OrderEditViewModel = orderEditViewModel;
            ViewBag.OrderViewModel = orderViewModel;

            return View(orderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrders(int id, OrderEditViewModel orderEditViewModel)
        {
            _logger.LogInformation("EditOrders POST called with id: {Id}", id);
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (id != orderEditViewModel.OrderID)
            {
                _logger.LogWarning("Mismatched order ID: {Id} != {OrderID}", id, orderEditViewModel.OrderID);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = await _context.Orders.FindAsync(id);
                    if (order == null || order.Status != "ordered")
                    {
                        _logger.LogWarning("Order not found or status is not 'ordered'.");
                        return NotFound();
                    }

                    // Update order properties
                    order.Date = orderEditViewModel.Date;
                    order.Quantity = orderEditViewModel.Quantity;
                    order.Status = orderEditViewModel.Status;

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Order updated successfully with id: {Id}", id);
                    return RedirectToAction(nameof(ViewOrders));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!OrderExists(orderEditViewModel.OrderID))
                    {
                        _logger.LogWarning("Order not found during update.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error occurred while updating order with id: {Id}", id);
                        throw;
                    }
                }
            }
            else
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }
                _logger.LogWarning("ModelState is invalid.");
            }

            // Return the same view model for editing
            return View(ViewBag.OrderEditViewModel);
        }


        private bool OrderExists(int id)
        {

            return _context.Orders.Any(e => e.OrderID == id);
        }

        public async Task<IActionResult> DeleteOrder(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                _logger.LogWarning($"Order with ID {id} not found.");
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order with ID {id} deleted.");

            return RedirectToAction(nameof(ViewOrders));
        }

        public ActionResult MedicationRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
        public IActionResult MaintainVitals()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
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
            ViewBag.Username = HttpContext.Session.GetString("Username");
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
            ViewBag.Username = HttpContext.Session.GetString("Username");
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
            ViewBag.Username = HttpContext.Session.GetString("Username");
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
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVitals(VitalsViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
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
