
using Dapper;
using Day_Hospital_e_prescribing_system.Helper;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class PharmacistController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly ILogger<PharmacistController> _logger;
        private readonly IConfiguration _config;
        private readonly CommonHelper _helper;
        private IDbConnection _connection;
        private readonly SurgeriesReportGenerator _surgeriesReportGenerator;
        public PharmacistController(ApplicationDbContext context, ILogger<PharmacistController> logger, IConfiguration config, SurgeriesReportGenerator surgeriesReportGenerator, IDbConnection connection)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _surgeriesReportGenerator = surgeriesReportGenerator;
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

        public async Task<ActionResult> Prescriptions(DateTime? startDate, DateTime? endDate)
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

            return View(prescriptions);

        }


        [HttpGet]
        public IActionResult ViewPrescription(int? id = null)
        {
            //try
            //{
                var prescription = _connection.QueryFirstOrDefault<PatientPrescriptionVM>(
                    "GetPatientPrescriptionByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure
                );

                if (prescription == null)
                {
                    return NotFound($"No prescription found with ID: {id}");
                }

                // Log successful retrieval
                _logger.LogInformation($"Retrieved prescription details for ID: {id}");

               

                return View(prescription);
            //}
            //catch (Exception ex)
            //{
            //    Log the exception
            //    _logger.LogError(ex, $"Error occurred while retrieving prescription details for ID: {id}");
            //    return StatusCode(500/*, "An unexpected error occurred."*/);
            //}
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
