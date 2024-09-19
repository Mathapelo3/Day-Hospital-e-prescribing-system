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

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Day_Hospital_e_prescribing_system.Helper;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    [Authorize]
    public class AnaesthesiologistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnaesthesiologistController> _logger;
       
        private readonly IConfiguration _configuration;
        private readonly CommonHelper _helper;
        private readonly OrderReportGenerator _orderReportGenerator;
        public AnaesthesiologistController(ApplicationDbContext context, ILogger<AnaesthesiologistController> logger,  IConfiguration configuration, OrderReportGenerator orderReportGenerator)
        {
            _context = context;
            _logger = logger;
            _orderReportGenerator = orderReportGenerator;
            _configuration = configuration;
            _helper = new CommonHelper(_configuration);

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
        public ActionResult ViewPatients(string searchString, DateTime? Date)
        {
            _logger.LogInformation("Entering ViewPatients action method");
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDate"] = Date?.ToString("yyyy-MM-dd");
            _logger.LogInformation("Received date: {Date}", Date?.ToString("yyyy-MM-dd"));

            var patientAdmission = _context.APatientViewModel.FromSqlRaw("EXEC sp_GetAdmittedPatients @SearchString, @Date",
                new SqlParameter("@SearchString", searchString ?? (object)DBNull.Value),
                new SqlParameter("@Date", Date ?? (object)DBNull.Value)).ToList();

            _logger.LogInformation("Exiting ViewPatients action method");

            return View(patientAdmission);


        }

        public async Task<ActionResult> BookedPatients(string searchString, DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation("BookedPatients action started");

            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("dd-MM-yyyy");
            ViewData["EndDate"] = endDate?.ToString("dd-MM-yyyy");

            var bookedPatients = from s in _context.Surgeries
                                 join p in _context.Patients on s.PatientID equals p.PatientID into patientGroup
                                 from p in patientGroup.DefaultIfEmpty()
                                 join n in _context.Nurses on s.NurseID equals n.NurseID into nurseGroup
                                 from n in nurseGroup.DefaultIfEmpty()
                                 join su in _context.Surgeons on s.SurgeonID equals su.SurgeonID into surgeonGroup
                                 from su in surgeonGroup.DefaultIfEmpty()
                                 join t in _context.Theatres on s.TheatreID equals t.TheatreID into theatreGroup
                                 from t in theatreGroup.DefaultIfEmpty()
                                 join b in _context.Bed on p.BedId equals b.BedId into bedGroup
                                 from b in bedGroup.DefaultIfEmpty()
                                 join w in _context.Ward on b.WardId equals w.WardId into wardGroup
                                 from w in wardGroup.DefaultIfEmpty()
                                 join c in _context.Surgery_TreatmentCodes on s.SurgeryID equals c.SurgeryID into codeGroup
                                 from c in codeGroup.DefaultIfEmpty()
                                 join u in _context.Users on n.UserID equals u.UserID into nurseUserGroup
                                 from u in nurseUserGroup.DefaultIfEmpty()
                                 join us in _context.Users on su.UserID equals us.UserID into surgeonUserGroup
                                 from us in surgeonUserGroup.DefaultIfEmpty()
                                 select new BookedPatientsViewModel
                                 {
                                     SurgeryID = s.SurgeryID,
                                     PatientID = p.PatientID,
                                     Patient = (p.Name ?? string.Empty) + " " + (p.Surname ?? string.Empty),
                                     Date = s.Date,
                                     Time = s.Time,
                                     WardName = w != null ? w.WardName : "N/A",  // Null check for Ward
                                     BedName = b != null ? b.BedName : "N/A",  // Null check for Bed
                                     Nurse = u != null ? u.Name + " " + u.Surname : "N/A",  // Null check for Nurse User
                                     Theatre = t != null ? t.Name : "N/A",  // Null check for Theatre
                                     Surgeon = us != null ? us.Name + " " + us.Surname : "N/A",  // Null check for Surgeon User
                                     Surgery_TreatmentCode = c != null ? c.Description : "N/A"  // Null check for Surgery_TreatmentCode
                                 };


            _logger.LogInformation($"Querying database for booked patients. Search string: {searchString}, Start date: {startDate}, End date: {endDate}");

            if (!String.IsNullOrEmpty(searchString))
            {
                bookedPatients = bookedPatients.Where(pa => pa.Patient.Contains(searchString));
                _logger.LogInformation($"Applying search filter: {searchString}");
            }

            if (startDate.HasValue)
            {
                bookedPatients = bookedPatients.Where(pa => pa.Date.Date >= startDate.Value.Date);
                _logger.LogInformation($"Applying start date filter: {startDate}");
            }

            if (endDate.HasValue)
            {
                bookedPatients = bookedPatients.Where(pa => pa.Date.Date <= endDate.Value.Date);
                _logger.LogInformation($"Applying end date filter: {endDate}");
            }


            var result = await bookedPatients.ToListAsync();
            _logger.LogInformation($"Retrieved {result.Count} booked patients");

            if (result.Count == 0)
            {
                _logger.LogWarning("No booked patients found");
            }

            return View(result); ;
        }
        public async Task<ActionResult> MedicalHistory(int id)
        {
            _logger.LogInformation("MedicalHistory action started for patient ID: {PatientId}", id);

            ViewBag.Username = HttpContext.Session.GetString("Username");

            var model = new PatientMHViewModel
            {
                PatientID = id // Ensure this is set
            };

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Query to get the patient's name and surname
                var patientCommand = new SqlCommand("SELECT Name, Surname FROM Patient WHERE PatientID = @PatientID", connection);
                patientCommand.Parameters.Add(new SqlParameter("@PatientID", id));

                using (var reader = await patientCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        model.Name = reader.GetString(reader.GetOrdinal("Name"));
                        model.Surname = reader.GetString(reader.GetOrdinal("Surname"));
                    }
                }

                // Move on to retrieve the medical history
                var command = new SqlCommand("GetPatientMedicalHistory", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add(new SqlParameter("@PatientID", id));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Vitals
                    model.Vitals = new List<VitalViewModel>();
                    while (await reader.ReadAsync())
                    {
                        model.Vitals.Add(new VitalViewModel
                        {
                            VitalDate = reader.GetDateTime(reader.GetOrdinal("VitalDate")),
                            VitalTime = reader.GetTimeSpan(reader.GetOrdinal("VitalTime")),
                            VitalHeight = reader.GetString(reader.GetOrdinal("VitalHeight")),
                            VitalWeight = reader.GetString(reader.GetOrdinal("VitalWeight")),
                            VitalName = reader.GetString(reader.GetOrdinal("VitalName")),
                            VitalValue = reader.GetString(reader.GetOrdinal("VitalValue")),
                            VitalNotes = reader.GetString(reader.GetOrdinal("VitalNotes"))
                        });
                    }

                    // Move reader to the next result set
                    await reader.NextResultAsync();

                    // Allergies
                    model.Allergies = new List<PAllergyViewModel>();
                    while (await reader.ReadAsync())
                    {
                        model.Allergies.Add(new PAllergyViewModel
                        {
                            AllergyName = reader.GetString(reader.GetOrdinal("AllergyName"))
                        });
                    }

                    // Move reader to the next result set
                    await reader.NextResultAsync();

                    // Chronic Conditions
                    model.Conditions = new List<PConditionViewModel>();
                    while (await reader.ReadAsync())
                    {
                        model.Conditions.Add(new PConditionViewModel
                        {
                            ConditionName = reader.GetString(reader.GetOrdinal("ConditionName"))
                        });
                    }

                    // Move reader to the next result set
                    await reader.NextResultAsync();

                    // Current Medications
                    model.Medications = new List<PMedicationViewModel>();
                    while (await reader.ReadAsync())
                    {
                        model.Medications.Add(new PMedicationViewModel
                        {
                            MedicationName = reader.GetString(reader.GetOrdinal("MedicationName"))
                        });
                    }
                }
            }

            // Assuming you have a view named MedicalHistory.cshtml
            return View(model);
        }

        public async Task<ActionResult> Prescriptions(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Prescriptions action called with id: {Id}", id);

            var model = new List<PPrescriptionViewModel>();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (var command = new SqlCommand("GetSPatientPrescriptions", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientID", id);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            model.Add(new PPrescriptionViewModel
                            {
                                PrescriptionID = reader.GetInt32(reader.GetOrdinal("PrescriptionID")),
                                Instruction = reader.GetString(reader.GetOrdinal("Instruction")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Quantity = reader.GetString(reader.GetOrdinal("Quantity")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Urgency = reader.GetBoolean(reader.GetOrdinal("Urgency")),
                                MedicationName = reader.GetString(reader.GetOrdinal("MedicationName")),
                                SurgeonFullName = reader.GetString(reader.GetOrdinal("SurgeonFullName")),
                                PatientFullName = reader.GetString(reader.GetOrdinal("PatientFullName")),
                                PatientID = reader.GetInt32(reader.GetOrdinal("PatientID"))
                            });
                        }
                    }
                }
            }

            // Log the count of retrieved prescriptions
            _logger.LogInformation("Retrieved {Count} prescriptions for patient ID: {Id}", model.Count, id);

            return View(model);
        }
        public IActionResult Orders(int id, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            List<OrderViewModel> orders = new List<OrderViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetOrdersForPatient", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PatientID", id);

                        // Add startDate and endDate parameters, handle nulls if not provided
                        cmd.Parameters.AddWithValue("@StartDate", (object)startDate ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderViewModel order = new OrderViewModel
                                {
                                    OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    MedicationName = reader.GetString(reader.GetOrdinal("Medication")),
                                    Quantity = reader.GetString(reader.GetOrdinal("Quantity")),
                                    Status = reader.GetString(reader.GetOrdinal("Status")),
                                    PatientID = reader.GetInt32(reader.GetOrdinal("PatientID")),
                                    PatientName = reader.GetString(reader.GetOrdinal("PatientName")),
                                    PatientSurname = reader.GetString(reader.GetOrdinal("PatientSurname"))
                                };
                                orders.Add(order);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exception
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }

            return View(orders);
        }
        
        public async Task<ActionResult> CaptureOrders(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var patient = await _context.Patients
                .Where(p => p.PatientID == id)
                .Select(p => new { p.Name, p.Surname })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                // Handle the case where the patient was not found
                return NotFound();
            }

            var medications = await _context.Medication.ToListAsync();
            var model = new OrderFormViewModel
            {
                PatientID = id,
                Name = patient.Name,
                Surname = patient.Surname,
                Medications = new SelectList(medications, "MedicationID", "Name"),
                SelectedMedications = new List<SPMedicationViewModel>(),
                Date = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CaptureOrders(OrderFormViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Starting to capture orders...");
            if (ModelState.IsValid)
            {
                var loggedInUserEmail = User.FindFirstValue(ClaimTypes.Email);
                _logger.LogInformation("Logged-in user's email: {Email}", loggedInUserEmail);

                if (string.IsNullOrEmpty(loggedInUserEmail))
                {
                    _logger.LogError("Logged-in user's email is null or empty.");
                    // Handle the error or return an error message
                }
                var anaesthesiologistID = _helper.GetAnaesthesiologistByEmail("SELECT a.AnaesthesiologistID, a.UserID, u.Username FROM Anaesthesiologist a INNER JOIN [User] u ON a.UserID = u.UserID WHERE u.Email = @Email", loggedInUserEmail).AnaesthesiologistID;


                if (model.SelectedMedications != null)
                {
                    foreach (var selectedMedication in model.SelectedMedications)
                    {
                        // Call the stored procedure or use EF to add each selected medication
                        _context.Database.ExecuteSqlRaw("EXEC InsertOrder @Date, @Quantity, @Status, @Urgency, @Administered, @QAdministered, @Notes, @PatientID, @AnaesthesiologistID, @MedicationID",
                            new SqlParameter("@Date", model.Date),
                            new SqlParameter("@Quantity", selectedMedication.Quantity),
                            new SqlParameter("@Status", "Ordered"), // Assuming the status is "Ordered" for new orders
                            new SqlParameter("@Urgency", model.Urgency),
                            new SqlParameter("@Administered", DBNull.Value), // Since Administered is not provided in the view model
                            new SqlParameter("@QAdministered", DBNull.Value), // Since QAdministered is not provided in the view model
                            new SqlParameter("@Notes", DBNull.Value), // Since Notes is not provided in the view model
                            new SqlParameter("@PatientID", model.PatientID),
                            new SqlParameter("@AnaesthesiologistID", anaesthesiologistID),
                            new SqlParameter("@MedicationID", selectedMedication.MedicationID)
                        );
                    }
                }

                // Redirect to the Orders page
                return RedirectToAction("ViewOrders", "Anaesthesiologist", new { startDate = model.Date, endDate = model.Date });
            }

            // If model is not valid, re-populate the medications list and return the view
            model.Medications = new SelectList(await _context.Medication.ToListAsync(), "MedicationID", "Name");
            return View(model);
        }


        public async Task<ActionResult> ViewOrders(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var parameters = new[] {
        new SqlParameter("@StartDate", startDate.HasValue ? startDate.Value : DBNull.Value),
        new SqlParameter("@EndDate", endDate.HasValue ? endDate.Value : DBNull.Value)
    };

            var orders = await _context.OrderViewModel.FromSqlRaw(
         "EXEC sp_GetOrdersWithoutAnaesthesiologistID @StartDate, @EndDate",
         parameters
     ).ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public IActionResult GenerateOrderReport(DateTime startDate, DateTime endDate)
        {
            var anaesthesiologistName = HttpContext.Session.GetString("Name");
            var anaesthesiologistSurname = HttpContext.Session.GetString("Surname");

            if (string.IsNullOrEmpty(anaesthesiologistName) || string.IsNullOrEmpty(anaesthesiologistSurname))
            {
                _logger.LogWarning("Anesthesiologist name or surname could not be retrieved from the session.");
                return BadRequest("Unable to retrieve anesthesiologist details.");
            }

            var reportStream = _orderReportGenerator.GenerateOrderReport(startDate, endDate, anaesthesiologistName, anaesthesiologistSurname);

            // Ensure the stream is not disposed prematurely
            if (reportStream == null || reportStream.Length == 0)
            {
                return NotFound(); // Or handle as appropriate
            }

            // Return the PDF file
            return File(reportStream, "application/pdf", "OrderReport.pdf");
        }

        public ActionResult EditOrders(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            _logger.LogInformation($"EditOrders action triggered with id: {id}");

            var order = _context.OrderEditViewModel
        .FromSqlRaw("EXEC sp_GetOrder @OrderId = {0}", id)
        .AsEnumerable()
        .FirstOrDefault();

            if (order == null)
            {
                _logger.LogWarning($"Order not found for id: {id}");
                return NotFound();
            }

            _logger.LogInformation($"ViewModel: {order}");

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOrders(OrderEditViewModel editOrder)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            if (ModelState.IsValid)
            {
                _logger.LogInformation($"Updating order with ID {editOrder.OrderID}, Date: {editOrder.Date}, Quantity: {editOrder.Quantity}");

                try
                {
                    await _context.Database.ExecuteSqlRawAsync("EXEC sp_UpdateOrder @OrderId = {0}, @Date = {1}, @Quantity = {2}",
                        editOrder.OrderID, editOrder.Date, editOrder.Quantity);

                    _logger.LogInformation("Order update successful.");
                    return RedirectToAction("ViewOrders");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating order.");
                    ModelState.AddModelError("", "Error updating order.");
                }
            }

            return View(editOrder);
        }

        private bool OrderExists(int id)
        {

            return _context.Orders.Any(e => e.OrderID == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Ordered")
            {
                return RedirectToAction("ViewOrders");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("ViewOrders");
        }

        [HttpGet]
        public async Task<ActionResult> MedicationRecords()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var patients = await _context.Patients
        .Select(p => new PatientDropDownViewModel
        {
            PatientID = p.PatientID,
            FullName = p.Name + " " + p.Surname
        })
        .ToListAsync();

            var model = new PSPatientOrderViewModel
            {
                Patients = patients,
                Orders = new List<PostSurgeryOrderViewModel>() // Initially empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MedicationRecords(PSPatientOrderViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            var patients = await _context.Patients
                .Select(p => new PatientDropDownViewModel
                {
                    PatientID = p.PatientID,
                    FullName = p.Name + " " + p.Surname
                })
                .ToListAsync();

            model.Patients = patients;

            if (model.PatientID > 0)
            {
                model.Orders = await _context.Orders
                    .Where(o => o.PatientID == model.PatientID && o.Status == "ordered")
                    .Select(o => new PostSurgeryOrderViewModel
                    {
                        OrderID = o.OrderID,
                        Date = o.Date,
                        MedicationName = o.Medication.Name,
                        Quantity = o.Quantity,
                        Status = o.Status,
                        Urgency = o.Urgency,
                        Administered = o.Administered ?? false,
                        QAdministered = o.QAdministered,
                        Notes = o.Notes
                    })
                    .ToListAsync();
            }
            else
            {
                model.Orders = new List<PostSurgeryOrderViewModel>();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMedicationRecords(PSPatientOrderViewModel model)
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            foreach (var order in model.Orders)
            {
                var existingOrder = await _context.Orders.FindAsync(order.OrderID);
                if (existingOrder != null)
                {
                    existingOrder.Administered = order.Administered;
                    existingOrder.QAdministered = order.QAdministered;
                    existingOrder.Notes = order.Notes;

                    _context.Update(existingOrder);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MedicationRecords", new { model.PatientID });
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

            var viewModel = new EditVitalsViewModel
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
