
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


        //Prescriptions
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


        [HttpPost]
        public async Task<IActionResult> OrderMedicines(MedicationOrderListVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Loop through the medications and insert only those with a valid Quantity
            foreach (var medication in model.Medications)
            {
                // Check if Quantity has a value and is greater than 0
                if (medication.Quantity.HasValue && medication.Quantity.Value > 0)
                {
                    var orderMedicine = new OrderMedicine
                    {
                        Date = DateTime.Now,            // Auto-generate the date
                        Urgency = true,                 // Set urgency to true
                        Quantity = medication.Quantity.Value, // Correctly access the Value
                        Status = "Ordered",             // Set status to "Ordered"
                        StockID = medication.StockID    // Use the StockID from the form
                    };

                    // Add the new order to the database context
                    _context.OrderMedicines.Add(orderMedicine);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the desired page after processing
            return RedirectToAction("PharmacistDashboard");
        }



        //private async Task<IActionResult> GetMedicationView()
        //{
        //    var medicationQuery = from dm in _context.DayHospitalMedication
        //                          join mt in _context.medicationType on dm.MedTypeId equals mt.MedTypeId
        //                          orderby dm.MedicationName ascending
        //                          select new DayHospitalMedicationVM
        //                          {
        //                              StockID = dm.StockID,
        //                              MedTypeId = dm.MedTypeId,
        //                              MedicationName = dm.MedicationName,
        //                              QtyLeft = dm.QtyLeft,
        //                              ReOrderLevel = dm.ReOrderLevel,
        //                              DosageForm = mt.DosageForm,
        //                              IsBelowReorderLevel = dm.QtyLeft < dm.ReOrderLevel
        //                          };

        //    var medications = await medicationQuery.ToListAsync();

        //    // Return the combined view model as an IActionResult
        //    var viewModel = new OrderMedicinesViewModel
        //    {
        //        Medications = medications,
        //        Order = new OrderMedicineVM() // Initialize a new order model
        //    };

        //    return View("OrderMedicines", viewModel); // Return the view with the combined view model
        //}



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
        public async Task<ActionResult> OrderedMedicine()
        {
            var orderQuery = from om in _context.OrderMedicines
                                  join dm in _context.DayHospitalMedication on om.StockID equals dm.StockID
                                  join mt in _context.medicationType on dm.MedTypeId equals mt.MedTypeId
                                  orderby om.Date ascending
                                  select new ViewModel.OrderMedicineVM
                                  {
                                      OrderId = om.OrderId,
                                      StockID = dm.StockID,
                                      MedTypeId = dm.MedTypeId,
                                      Date = om.Date,
                                      MedicationName = dm.MedicationName,
                                      DosageForm = mt.DosageForm,
                                      Quantity = om.Quantity,
                                      Status  = om.Status,
                                      QtyReceived = dm.QtyReceived,
                                      QtyLeft = dm.QtyLeft,
                                      Urgency = om.Urgency
                                      
                                     
                                  };

            var orders = await orderQuery.ToListAsync();

            return View(orders);

            
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
                // Find the corresponding medication in the database
                var existingMedication = await _context.DayHospitalMedication.FindAsync(medication.StockID);

                if (existingMedication != null)
                {
                    // Update QtyReceived and QtyLeft
                    existingMedication.QtyReceived += medication.Quantity; // Increment QtyReceived by the ordered quantity
                    existingMedication.QtyLeft += medication.Quantity;     // Increment QtyLeft by the ordered quantity
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect or return a response as needed
            return RedirectToAction("OrderedMedicine"); // Change to your desired action
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
