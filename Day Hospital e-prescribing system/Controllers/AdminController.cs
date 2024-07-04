﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Day_Hospital_e_prescribing_system.Models;
using Day_Hospital_e_prescribing_system.ViewModels;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Day_Hospital_e_prescribing_system.ViewModel;

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
            var specializations = _context.Specializations.Select(s => new SelectListItem
            {
                Value = s.SpecializationID.ToString(),
                Text = s.Type
            }).ToList();

            var viewModel = new UserViewModel
            {
                Specializations = specializations
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMedicalProfessional(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    ContactNo = model.ContactNo,
                    HCRNo = model.HCRNo,
                    Username = model.Username,
                    Password = EncryptPassword(model.Password),
                    AdminID = 1, // Assuming there is only one admin user
                    SpecializationID = model.SpecializationID
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Add to specific specialization table
                switch (model.SpecializationID)
                {
                    case 1: // Nurse
                        _context.Nurses.Add(new Nurse { UserID = user.UserID });
                        break;
                    case 2: // Surgeon
                        _context.Surgeons.Add(new Surgeon { UserID = user.UserID });
                        break;
                    case 3: // Pharmacist
                        _context.Pharmacists.Add(new Pharmacist { UserID = user.UserID });
                        break;
                    case 4: // Anaesthesiologist
                        _context.Anaesthesiologists.Add(new Anaesthesiologist { UserID = user.UserID });
                        break;
                }

                _context.SaveChanges();
                return RedirectToAction("Index"); // Redirect to a suitable page after success
            }

            // If we got this far, something failed, redisplay form
            model.Specializations = _context.Specializations.Select(s => new SelectListItem
            {
                Value = s.SpecializationID.ToString(),
                Text = s.Type
            }).ToList();

            return View(model);
        }

        private string EncryptPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
            

        public IActionResult TheatreRecords()
        {
            var theatre = _context.Theatres.ToList();
            ViewBag.Theatre = theatre;

            return View();
        }
        public IActionResult WardRecords()
        {
            var ward = _context.Wards.ToList();
            ViewBag.Ward = ward;

            return View();
        }
        public IActionResult ChronicConditionRecords()
        {
            var condition = _context.Conditions.ToList().OrderBy(c => c.Name);
            ViewBag.Condition = condition;

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
            var icd = _context.ICDCodes.ToList();
            var activeIngredients = _context.Active_Ingredient.ToList();

            var icdselectList = new SelectList(icd, "ICD_ID", "Description");
            ViewBag.ICDCodes = icdselectList;

            var activeIngredientSelectList = new SelectList(activeIngredients, "Active_IngredientID", "Description");
            ViewBag.ActiveIngredients = activeIngredientSelectList;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveContraIndication(int[] selectedICD_ID, int[] selectedActive_IngredientID)
        {
            var icd = await _context.ICDCodes.Where(g => selectedICD_ID.Contains(g.ICD_ID)).ToListAsync();
            var activeIngredients = await _context.Active_Ingredient.Where(d => selectedActive_IngredientID.Contains(d.Active_IngredientID)).ToListAsync();

            foreach (var icds in icd)
            {
                foreach (var ai in activeIngredients)
                {
                    var existingEntry = await _context.Medication_Interaction.FirstOrDefaultAsync(d => d.Active_IngredientID == ai.Active_IngredientID && d.ICD_ID == icds.ICD_ID);
                    if (existingEntry != null)
                    {
                        // Update existing entry if needed
                    }
                    else
                    {
                        // Create new entry if not existent
                        _context.Medication_Interaction.Add(new Medication_Interaction { Active_IngredientID = ai.Active_IngredientID, ICD_ID = icds.ICD_ID });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ContraIndicationRecords", new { selectedICD_ID, selectedActive_IngredientID});

        }

        public async Task<IActionResult> ContraIndicationRecords()
        {

            var icdWai = await _context.ICDCodes
               .Include(icd => icd.Medication_Interaction)
                .ThenInclude(medication_interaction => medication_interaction.Active_Ingredient)
               .Select(icd => new ICD_CodesVM
               {
                   ICD_ID = icd.ICD_ID,
                   Description = icd.Description,
                   Active_Ingredient = icd.Medication_Interaction.Select(dxg => dxg.Active_Ingredient.Description).ToList()
               })
               .ToListAsync();

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
