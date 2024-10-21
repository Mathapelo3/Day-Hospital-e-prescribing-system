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
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class PharmacistController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly ILogger<PharmacistController> _logger;
        private readonly IConfiguration _config;
        private readonly CommonHelper _helper;
        private IDbConnection _connection;
        private readonly PharmacistReportGenerator _reportGenerator;


        public PharmacistController(ApplicationDbContext context, ILogger<PharmacistController> logger, IConfiguration config, PharmacistReportGenerator reportGenerator, IDbConnection connection)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _reportGenerator = reportGenerator;
            _helper = new CommonHelper(_config); // Consider changing this to dependency injection
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
        public async Task<IActionResult> GetDispensaryReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _connection.Open();

                var reportData = await _connection.QueryAsync<DispenseReportDataViewModel>(
                    "GetDispensaryReportData",
                    new { StartDate = startDate, EndDate = endDate },
                    commandType: CommandType.StoredProcedure
                );

                if (!reportData.Any())
                {
                    ViewBag.SuccessMessage = "No data found for the given date range.";
                    return View(reportData); // Return the empty model to the view
                }

                // Return the model to the view
                return View(reportData);
            }
            catch (Exception ex)
            {
                // Handle the error
                _logger.LogError(ex, "Error retrieving dispensary report data.");
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }

        [HttpPost]
        public IActionResult PharmacistReportGenerator(DateTime startDate, DateTime endDate, string pharmacistName, string pharmacistSurname)
        {
            try
            {
                _logger.LogInformation("Generating report from {StartDate} to {EndDate} for {PharmacistName} {PharmacistSurname}", startDate, endDate, pharmacistName, pharmacistSurname);

                // Generate the report and get the MemoryStream
                var reportStream = _reportGenerator.GenerateDispensaryReport(startDate, endDate, pharmacistName, pharmacistSurname);

                // Return the PDF as a file download
                return File(reportStream.ToArray(), "application/pdf", "PharmacistReport.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating the pharmacist report.");
                return StatusCode(500, "Internal server error");
            }
        }


        public async Task<ActionResult> DispensePrescriptions(DateTime? startDate, string message)
        {
            var sqlQuery = "EXEC UrgentPrescriptions @StartDate = {0}";

            var prescriptions = await _context.prescriptionViewModels
                .FromSqlInterpolated($"EXEC UrgentPrescriptions @StartDate = {startDate ?? (object)DBNull.Value}")
                .ToListAsync();

            ViewBag.SuccessMessage = message;

            return View(prescriptions);
        }

        public async Task<ActionResult> AllPrescriptions(string status, string message, int page = 1, int pageSize = 10)
        {
            // Retrieve all prescriptions using the stored procedure
            var prescriptions = await _context.prescriptionViewModels
                .FromSqlInterpolated($"EXEC AllPrescriptions @Status = {status ?? (object)DBNull.Value}")
                .ToListAsync(); // Fetch the data into a List

            // Pagination: calculate the number of records to skip based on the current page and page size
            var skip = (page - 1) * pageSize;

            // Perform pagination in-memory
            var paginatedPrescriptions = prescriptions.Skip(skip).Take(pageSize).ToList();

            // Get the total count of prescriptions for pagination info
            var totalCount = prescriptions.Count;

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pass pagination data to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SuccessMessage = message;

            return View(paginatedPrescriptions);
        }







        [HttpGet]
        public async Task<IActionResult> ViewPrescription(int? id = null)
        {
            // Check if the prescription ID is provided
            if (!id.HasValue)
            {
                return NotFound("No prescription ID provided.");
            }

            // Fetch prescription data along with allergies and conditions
            var viewModel = await GetPatientPrescriptionWithRelatedData(id.Value);

            // Check if the view model is null or has no prescriptions
            if (viewModel == null || !viewModel.Prescription.Any())
            {
                return NotFound($"No prescription found with ID: {id}");
            }

            // Check if each prescription is successful based on quantities
            foreach (var prescription in viewModel.Prescription)
            {
                prescription.IsSuccess = prescription.Qty < prescription.QtyLeft; // Adjust as necessary
            }

            // Log alert messages directly from the view model
            if (!string.IsNullOrEmpty(viewModel.AllergyAlert))
            {
                _logger.LogInformation($"Allergy Alert Value: {viewModel.AllergyAlert}");
            }

            if (!string.IsNullOrEmpty(viewModel.ContraIndicationAlert))
            {
                _logger.LogInformation($"Contra Indication Alert Value: {viewModel.ContraIndicationAlert}");
            }

            // Log the retrieval of the prescription details
            _logger.LogInformation($"Retrieved patient prescription details for ID: {id}");

            // Return the view with the populated view model
            return View(viewModel);
        }



        // Existing method to get allergy alert messages
        private string GetAllergyAlertMessage(IEnumerable<PatientAllergiesViewModel> allergies)
        {
            if (allergies == null || !allergies.Any())
                return null;

            var alertMessages = allergies.Select(a => a.AllergyName);
            var message = "Warning: The selected medication contains the following active ingredients that the patient is allergic to: " + string.Join(", ", alertMessages);

            // Log the generated allergy alert message
            _logger.LogInformation($"Generated Allergy Alert: {message}");
            return message;
        }

        private string GetContraIndicationAlertMessage(IEnumerable<PatientConditionsViewModel> conditions)
        {
            if (conditions == null || !conditions.Any())
                return null;

            var alertMessages = conditions.Select(c => c.ConditionName);
            var message = "Warning: The following conditions may cause conflicts with the prescribed medication: " + string.Join(", ", alertMessages);

            // Log the generated contraindication alert message
            _logger.LogInformation($"Generated Contraindication Alert: {message}");
            return message;
        }

        private async Task<PatientPrescriptionWithRelatedDataVM> GetPatientPrescriptionWithRelatedData(int id)
        {
            try
            {
                // Retrieve prescriptions
                var prescriptions = await _connection.QueryAsync<PatientPrescriptionVM>(
                    "GetPatientPrescriptionByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                // Check if there are any prescriptions
                if (prescriptions == null || !prescriptions.Any())
                {
                    return null; // Early return if no prescriptions found
                }

                // Retrieve PatientID and StockID from the first prescription
                var patientId = prescriptions.First().PatientID;
                var stockId = prescriptions.First().StockID;

                // Allergy alert logic
                var allergyAlertParams = new DynamicParameters();
                allergyAlertParams.Add("@PatientID", patientId);
                allergyAlertParams.Add("@StockID", stockId);
                allergyAlertParams.Add("@AlertMessage", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                // Call the Allergy Alert stored procedure
                await _connection.ExecuteAsync(
                    "AllergyAlertForPrescription",
                    allergyAlertParams,
                    commandType: CommandType.StoredProcedure);

                var allergyAlertValue = allergyAlertParams.Get<string>("@AlertMessage");
                _logger.LogInformation($"Allergy Alert Value: {allergyAlertValue}"); // Log the allergy alert value

                // Check if the allergy alert is null or empty
                if (string.IsNullOrEmpty(allergyAlertValue))
                {
                    _logger.LogWarning($"No allergy alert found for PatientID: {patientId} and StockID: {stockId}");
                }

                // Contraindication alert logic
                var contraIndicationAlertParams = new DynamicParameters();
                contraIndicationAlertParams.Add("@PatientID", patientId);
                contraIndicationAlertParams.Add("@StockID", stockId);
                contraIndicationAlertParams.Add("@AlertMessage", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                // Call the Contraindication Alert stored procedure
                await _connection.ExecuteAsync(
                    "ContraIndicationAllertForPrescription",
                    contraIndicationAlertParams,
                    commandType: CommandType.StoredProcedure);

                var contraIndicationAlertValue = contraIndicationAlertParams.Get<string>("@AlertMessage");
                _logger.LogInformation($"Contra Indication Alert Value: {contraIndicationAlertValue}"); // Log the contraindication alert value

                // Check if the contraindication alert is null or empty
                if (string.IsNullOrEmpty(contraIndicationAlertValue))
                {
                    _logger.LogWarning($"No contraindication alert found for PatientID: {patientId} and StockID: {stockId}");
                }

                // Retrieve other related data as needed
                var conditions = await _connection.QueryAsync<PatientConditionsViewModel>(
                    "GetPatientConditionsByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var vitals = await _connection.QueryAsync<PatientVitalsViewModel>(
                    "GetPatientVitalsByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                var medication = await _connection.QueryAsync<PatientMedicationVM>(
                    "GetPatientMedicationByPrescriptionID",
                    new { PrescriptionID = id },
                    commandType: CommandType.StoredProcedure);

                // Return the complete view model with alerts
                return new PatientPrescriptionWithRelatedDataVM
                {
                    Prescription = prescriptions,
                    Allergies = new List<PatientAllergiesViewModel>(), // Placeholder for allergies, implement as needed
                    Conditions = conditions,
                    Vitals = vitals,
                    Medications = medication,
                    AllergyAlert = allergyAlertValue,
                    ContraIndicationAlert = contraIndicationAlertValue // Set the contraindication alert
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient prescription details for ID: {id}");
                throw;
            }

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

        [HttpPost]
        public async Task<IActionResult> RejectPrescription(int prescriptionId)
        {
            try
            {
                // Call the stored procedure to reject the prescription
                await _connection.ExecuteAsync(
                    "RejectPrescription",
                    new { PrescriptionID = prescriptionId },
                    commandType: CommandType.StoredProcedure
                );

                _logger.LogInformation($"Prescription {prescriptionId} rejected successfully.");
                return RedirectToAction("Prescriptions"); // Redirect to the appropriate view after rejection
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting Prescription {prescriptionId}: {ex.Message}");
                return View("Error");
            }

        }

        //[HttpGet]
        //public async Task<ActionResult> AllPrescriptions(DateTime? startDate, DateTime? endDate)
        //{

        //    ViewBag.Username = HttpContext.Session.GetString("Username");
        //    // Get the logged-in user's ID
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized("User not authenticated");
        //    }

        //    // Convert userId to int if necessary
        //    int surgeonId;
        //    if (!int.TryParse(userId, out surgeonId))
        //    {
        //        return BadRequest("Invalid user ID");
        //    }

        //    // Get the surgeon's ID based on the user ID
        //    var surgeon = await _context.Surgeons
        //        .FirstOrDefaultAsync(s => s.SurgeonID == surgeonId);

        //    if (surgeon == null)
        //    {
        //        return NotFound("Surgeon not found");
        //    }

        //    // Call the stored procedure
        //    var surgeryDetails = await _context.Set<PrescriptionViewModel>().FromSqlRaw(
        //        "EXEC [dbo].[GetDispensaryReportData] @StartDate, @EndDate",
        //        new SqlParameter("@StartDate", startDate ?? (object)DBNull.Value),
        //        new SqlParameter("@EndDate", endDate ?? (object)DBNull.Value)

        //    ).ToListAsync();

        //    ViewBag.StartDate = startDate;
        //    ViewBag.EndDate = endDate;

        //    return View(surgeryDetails);
        //}

       
        






        private byte[] GeneratePdfReport(List<DispenseReportDataViewModel> reportData)
        {
            using (var memoryStream = new MemoryStream())
            {
                var pdfWriter = new PdfWriter(memoryStream);
                var pdfDocument = new PdfDocument(pdfWriter);
                var document = new Document(pdfDocument);

                document.Add(new Paragraph("Dispensary Report").SetFontSize(20));

                foreach (var item in reportData)
                {
                    document.Add(new Paragraph($"{item.Date.ToShortDateString()} - {item.PatientName} {item.PatientSurname} - {item.SurgeonName} - {item.MedicationName} - {item.Quantity} - {item.Status} "));
                }

                document.Close();

                // Return the byte array of the PDF document
                return memoryStream.ToArray();
            }
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

       

    }
}
