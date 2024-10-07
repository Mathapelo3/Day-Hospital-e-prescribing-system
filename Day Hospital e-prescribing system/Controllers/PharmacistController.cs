
using Dapper;
using Day_Hospital_e_prescribing_system.Helper;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class PharmacistController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly ILogger<PharmacistController> _logger;
        private readonly IConfiguration _config;
        private readonly CommonHelper _helper;
        private IDbConnection _connection;
        private readonly PharmacistReportGenerator _pharmacistReportGenerator;
        
        public PharmacistController(ApplicationDbContext context, ILogger<PharmacistController> logger, IConfiguration config, PharmacistReportGenerator pharmacistReportGenerator, IDbConnection connection)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _pharmacistReportGenerator = pharmacistReportGenerator;
            _helper = new CommonHelper(_config);
            _connection = connection;   
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

        public async Task<ActionResult> Prescriptions(DateTime? startDate, DateTime? endDate, string message)
        {

            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentDate"] = startDate?.ToString("dd-MM-yyyy");


            var prescriptionsQuery = from pr in _context.Prescriptions
                                     join p in _context.Patients on pr.PatientID equals p.PatientID
                                     join s in _context.Surgeons on pr.SurgeonID equals s.SurgeonID
                                     join u in _context.Users on s.UserID equals u.UserID
                                     join d in _context.DayHospitalMedication on pr.MedicationID equals d.StockID
                                     where pr.Status != "Dispensed"
                                     select new ViewModel.PrescriptionViewModel
                                     {
                                         PrescriptionID = pr.PrescriptionID,
                                         Medication = d.MedicationName,
                                         Instruction = pr.Instruction,
                                         Date = pr.Date,
                                         Quantity = pr.Quantity,
                                         Status = pr.Status,
                                         PatientID = pr.PatientID,
                                         Name = $"{p.Name} {p.Surname}",
                                         Surgeon = $"{u.Username} {u.Surname}",
                                         Urgency = pr.Urgency

                                     };

            if (startDate.HasValue)
            {
                prescriptionsQuery = prescriptionsQuery.Where(pa => pa.Date.Date >= startDate.Value.Date);
            }

            var prescriptions = await prescriptionsQuery.ToListAsync();

            ViewBag.SuccessMessage = message;

            return View(prescriptions);

        }


        [HttpGet]
        public IActionResult ViewPrescription(int? id = null)
        {
            if (!id.HasValue)
            {
                return NotFound("No prescription ID provided.");
            }

            var viewModel = GetPatientPrescriptionWithRelatedData(id.Value);

            if (viewModel == null || !viewModel.Prescription.Any())
            {
                return NotFound($"No prescription found with ID: {id}");
            }

            _logger.LogInformation($"Retrieved patient prescription details for ID: {id}");

            return View(viewModel);
        }


        private PatientPrescriptionWithRelatedDataVM GetPatientPrescriptionWithRelatedData(int id)
        {
            try
            {
                var prescriptions = _connection.Query<PatientPrescriptionVM>(
                    "GetPatientPrescriptionByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var allergies = _connection.Query<PatientAllergiesViewModel>(
                    "GetPatientAllergiesByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var conditions = _connection.Query<PatientConditionsViewModel>(
                    "GetPatientConditionsByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var vitals = _connection.Query<PatientVitalsViewModel>(
                    "GetPatientVitalsByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var medication = _connection.Query<PatientMedicationVM>(
                    "GetPatientMedicationByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                return new PatientPrescriptionWithRelatedDataVM
                {
                    Prescription = prescriptions,
                    Allergies = allergies,
                    Conditions = conditions,
                    Vitals = vitals,
                    Medications = medication
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient prescription details for ID: {id}");
                throw;
            }
        }

        [HttpPost]
        public IActionResult DispensePrescription(int prescriptionId)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    connection.Open();

                    var result = connection.Execute(@"EXECUTE DispensePrescription @PrescriptionID", new { PrescriptionID = prescriptionId });

                    if (result > 0)
                    {
                        _logger.LogInformation($"Prescription {prescriptionId} dispatched successfully.");
                        return RedirectToAction("Prescriptions,Pharmacist", new { id = prescriptionId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to dispatch prescription.");
                        return View("ViewPrescription", GetPatientPrescriptionWithRelatedData(prescriptionId));
                    }
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Database error occurred: {ex.Message}");
                return View("ViewPrescription", GetPatientPrescriptionWithRelatedData(prescriptionId));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                return View("ViewPrescription", GetPatientPrescriptionWithRelatedData(prescriptionId));
            }
        }


        public async Task<ActionResult> AllPrescriptions(DateTime startDate, DateTime endDate)
        {
            var pharmacistName = HttpContext.Session.GetString("Name");
            var pharmacistSurname = HttpContext.Session.GetString("Surname");

            if (string.IsNullOrEmpty(pharmacistName) || string.IsNullOrEmpty(pharmacistSurname))
            {
                _logger.LogWarning("Anesthesiologist name or surname could not be retrieved from the session.");
                return BadRequest("Unable to retrieve anesthesiologist details.");
            }

            var reportStream = _pharmacistReportGenerator.GenerateDispensaryReport(startDate, endDate, pharmacistSurname, pharmacistSurname);

            // Ensure the stream is not disposed prematurely
            if (reportStream == null || reportStream.Length == 0)
            {
                return NotFound(); // Or handle as appropriate
            }

            // Return the PDF file
            return File(reportStream, "application/pdf", "OrderReport.pdf");
        }


        public IActionResult RejectPrescription()
        {
            return PartialView("_RejectPrescriptionPartialView");
        }

        public IActionResult ListDispensedPrescriptions()
        {
            return View();
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
