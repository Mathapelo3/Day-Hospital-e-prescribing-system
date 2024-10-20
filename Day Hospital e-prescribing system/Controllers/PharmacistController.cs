
using Dapper;
using Day_Hospital_e_prescribing_system.Helper;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModel;
using Day_Hospital_e_prescribing_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MailKit.Net.Smtp;
using System.Data;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using MailKit.Security;
using System.Configuration;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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
        public async Task<ActionResult> MedicineList()
        {
            var medicationQuery = from dm in _context.DayHospitalMedication
                                  join mt in _context.medicationType on dm.MedTypeId equals mt.MedTypeId
                                  orderby dm.MedicationName ascending
                                  select new ViewModel.DayHospitalMedicationVM
                                  {
                                      StockID = dm.StockID,
                                      MedTypeId = dm.MedTypeId,
                                      MedicationName = dm.MedicationName,
                                      QtyLeft = dm.QtyLeft,
                                      ReOrderLevel = dm.ReOrderLevel,
                                      DosageForm = mt.DosageForm,
                                      IsBelowReorderLevel = dm.QtyLeft < dm.ReOrderLevel


                                  };

           

            var medications = await medicationQuery.ToListAsync();

            
            return View(medications);
        }

        public IActionResult ReceiveMedicine()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddMedicine()
        {
            var MedicineType = _context.medicationType
             .OrderBy(mt => mt.DosageForm) // Change this property as necessary for sorting
             .ToList();

            var ActiveIngredients = _context.Active_Ingredient
                .OrderBy(ai => ai.Description) // Change this property as necessary for sorting
                .ToList();
            var Schedule = _context.Medication_Schedule
                .OrderBy(s => s.ScheduleId) // Change this property as necessary for sorting
                .ToList();

            ViewBag.MedicineType = MedicineType;
            ViewBag.ActiveIngredients = ActiveIngredients;
            ViewBag.Schedule = Schedule;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMedicine(AddMedicineViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new medicine entry
                var newMedicine = new DayHospitalMedication
                {
                    ReOrderLevel = model.ReOrderLevel,
                    MedicationName = model.MedicationName,
                    Schedule = model.Schedule,
                    MedTypeId = model.MedTypeId
                };

                // Add the new medicine to the context
                await _context.DayHospitalMedication.AddAsync(newMedicine);
                await _context.SaveChangesAsync(); // Save changes to get the StockID

                // Get the StockID of the newly added medicine
                var stockId = newMedicine.StockID;

                // Create and add active ingredients associated with the new medicine
                var activeIngredients = new List<DayHospitalMed_ActiveIngredients>();
                foreach (var ingredient in model.ActiveIngredients)
                {
                    activeIngredients.Add(new DayHospitalMed_ActiveIngredients
                    {
                        Active_IngredientID = ingredient.Active_IngredientID,
                        StockID = stockId,
                        Strenght = ingredient.Strength
                    });
                }

                // Add active ingredients to the context
                await _context.dayHospitalMed_ActiveIngredients.AddRangeAsync(activeIngredients);
                await _context.SaveChangesAsync(); // Save changes to persist active ingredients

                return RedirectToAction("PharmacistDashboard"); // Redirect or return a view as needed
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }

        //pRESCRIPRIONS
        public async Task<ActionResult> Prescriptions(DateTime? startDate, string message)
        {
            var sqlQuery = "EXEC GetPrescriptions @StartDate = {0}";

            var prescriptions = await _context.prescriptionViewModels
                .FromSqlInterpolated($"EXEC GetPrescriptions @StartDate = {startDate ?? (object)DBNull.Value}")
                .ToListAsync();

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

            foreach (var prescription in viewModel.Prescription)
            {
                prescription.IsSuccess = prescription.Qty < prescription.QtyLeft; // Assuming these properties exist
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
                   
                    /*AllergyAlert = allergyAlertMessage */// Set the alert message here
                };
            
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient prescription details for ID: {id}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckAllergy(int patientId, int stockId)
        {
            var parameter = new SqlParameter("@AlertMessage", SqlDbType.NVarChar, -1)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.AllergyCheckForSurgeon @PatientID, @StockID, @AlertMessage OUT",
                new SqlParameter("@PatientID", patientId),
                new SqlParameter("@StockID", stockId),
                parameter
            );

            string alertMessage = parameter.Value == DBNull.Value ? null : (string)parameter.Value;

            return Json(new { hasAllergy = !string.IsNullOrEmpty(alertMessage), message = alertMessage });
        }



        [HttpPost]
        public async Task<ActionResult> DispensePrescription(int prescriptionId)
        {
            // Define your connection string (assuming it's stored in a configuration)
            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                // Open the connection
                await connection.OpenAsync();

                // Execute the stored procedure to dispense the prescription
                var result = await connection.ExecuteAsync("EXECUTE DispensePrescription @PrescriptionID", new { PrescriptionID = prescriptionId });

                // Optionally, check the result or handle any exceptions
                if (result > 0) // Assuming a positive result indicates success
                {
                    // Redirect to the Prescriptions view after dispensing
                    return RedirectToAction("Prescriptions", new { message = "Prescription dispensed successfully!" });
                }
                else
                {
                    // Handle the case where the dispensing was not successful
                    ViewBag.Succsses = "Prescription dispensed successfully!n.";
                    return RedirectToAction("Prescriptions", new { message = ViewBag.Succsses });
                }
            }
        }


        [HttpGet]
        public async Task<ActionResult> AllPrescriptions(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            // Get the logged-in user's ID
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            // Convert userId to int if necessary
            int surgeonId;
            if (!int.TryParse(userId, out surgeonId))
            {
                return BadRequest("Invalid user ID");
            }

            // Get the surgeon's ID based on the user ID
            var surgeon = await _context.Surgeons
                .FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);

            if (surgeon == null)
            {
                return NotFound("Surgeon not found");
            }

            // Call the stored procedure
            var surgeryDetails = await _context.Set<PrescriptionViewModel>().FromSqlRaw(
                "EXEC [dbo].[GetDispensaryReportData] @StartDate, @EndDate",
                new SqlParameter("@StartDate", startDate ?? (object)DBNull.Value),
                new SqlParameter("@EndDate", endDate ?? (object)DBNull.Value)
              
            ).ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(surgeryDetails);
        }

        [HttpGet]
        public IActionResult PharmacistReportGenerator(DateTime startDate, DateTime endDate)
        {
            var surgeonName = HttpContext.Session.GetString("Name");
            var surgeonSurname = HttpContext.Session.GetString("Surname");

            if (string.IsNullOrEmpty(surgeonName) || string.IsNullOrEmpty(surgeonSurname))
            {
                _logger.LogWarning("Surgeon name or surname could not be retrieved from the session.");
                return BadRequest("Unable to retrieve surgeon details.");
            }

            var reportStream = _pharmacistReportGenerator.GenerateDispensaryReport(startDate, endDate, surgeonName, surgeonSurname);

            // Ensure the stream is not disposed prematurely
            if (reportStream == null || reportStream.Length == 0)
            {
                return NotFound(); // Or handle as appropriate
            }

            // Return the PDF file
            return File(reportStream, "application/pdf", "SurgeriesReport.pdf");
        }

        [HttpGet]
        public IActionResult RejectPrescription(int? id = null)
        {
            if (!id.HasValue)
            {
                return NotFound("No prescription ID provided.");
            }

            var viewModel = GetPatientPrescriptionWithRelatedData(id.Value);

            // Check if the viewModel is null or has no prescriptions
            if (viewModel == null || viewModel.Prescription == null || !viewModel.Prescription.Any())
            {
                return NotFound($"No prescription found with ID: {id}");
            }

            // This block assumes you want to perform some action on each prescription.
            foreach (var prescription in viewModel.Prescription)
            {
                // Example logic, modify as needed
                prescription.IsSuccess = prescription.Qty < prescription.QtyLeft; // Assuming these properties exist
            }

            _logger.LogInformation($"Retrieved patient prescription details for ID: {id}");

            return View(viewModel);
        }


        public IActionResult ListDispensedPrescriptions()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> OrderMedicines()
        {
            var medicationQuery = from dm in _context.DayHospitalMedication
                                  join mt in _context.medicationType on dm.MedTypeId equals mt.MedTypeId
                                  orderby dm.MedicationName ascending
                                  select new ViewModel.DayHospitalMedicationVM
                                  {
                                      StockID = dm.StockID,
                                      MedTypeId = dm.MedTypeId,
                                      MedicationName = dm.MedicationName,
                                      QtyLeft = dm.QtyLeft,
                                      ReOrderLevel = dm.ReOrderLevel,
                                      DosageForm = mt.DosageForm,
                                      IsBelowReorderLevel = dm.QtyLeft < dm.ReOrderLevel
                                  };

            var medications = await medicationQuery.ToListAsync();

            return View(medications); // Return the populated model
        }


        



        private async Task SendOrderEmail(string emailBody)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Hospital E-Prescribing System", "system@example.com"));
            emailMessage.To.Add(new MailboxAddress("Purchase Manager", "purchase.manager@example.com"));
            emailMessage.Subject = "Medicine Order";
            emailMessage.Body = new TextPart("plain") { Text = emailBody };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.mailserver.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("your_email@example.com", "your_password");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }


        [HttpGet]
        public async Task<IActionResult> OrderedMedicine()
        {
            var orders = await _context.orderMedicineVMs
                .FromSqlRaw("EXEC GetOrderMedicines")
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderByStatus(int? id = null)
        {
            // Check if orderId is null
            if (id == null)
            {
                // Handle the case where no orderId is provided (return empty view or all orders)
                Console.WriteLine("OrderId is null.");
                return View(new List<OrderMedicineViewModel>()); // Or handle appropriately
            }

            // If orderId is not null, run the SQL query
            var orders = await _context.orderMedicineVMs
                .FromSqlRaw("EXEC GetOrderByStatus @OrderId", new SqlParameter("@OrderId", id.Value))
                .ToListAsync();

            if (!orders.Any())
            {
                Console.WriteLine($"No orders found for OrderId: {id}");
            }
            else
            {
                Console.WriteLine($"Found {orders.Count} orders for OrderId: {id}");
            }

            var viewModel = orders.Select(o => new OrderMedicineViewModel
            {
                OrderId = o.OrderId,
                Date = o.Date,
                Quantity = o.Quantity,
                Status = o.Status,
                StockID = o.StockID,
                MedicationName = o.MedicationName,
                MedTypeId = o.MedTypeId,
                DosageForm = o.DosageForm,
                QtyLeft = o.QtyLeft,
                QtyReceived = o.QtyReceived
            }).ToList();

            if (!viewModel.Any())
            {
                Console.WriteLine("No data in viewModel.");
            }

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> UpdateMedicationQuantities(List<UpdateMedicationQuantityVM> medications)
        {
            if (medications == null || !medications.Any())
            {
                return BadRequest("No medications to update.");
            }

            foreach (var medication in medications)
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateMedicationQuantities @StockID, @Quantity",
                    new SqlParameter("@StockID", medication.StockID),
                    new SqlParameter("@Quantity", medication.Quantity));
            }

            return RedirectToAction("OrderedMedicine");
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
