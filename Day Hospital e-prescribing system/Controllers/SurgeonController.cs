﻿using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class SurgeonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SurgeonController> _logger;
        public SurgeonController(ApplicationDbContext context, ILogger<SurgeonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ActionResult Dashboard()
        {
            string username = HttpContext.Session.GetString("UserName"); // Use HttpContext.Session

            // Pass the username to the view
            ViewBag.Username = username;
            return View();
        }

        public IActionResult Prescriptions()
        {
            var prescription = _context.Prescriptions.ToList();
            ViewBag.Prescription = prescription;

            return View();
        }

        public IActionResult Patients()
        {
            var patients = _context.Patients.ToList();
            ViewBag.Patient = patients;

            return View();
        }

        public IActionResult AddPatients()
        {
            return View();
        }


        public IActionResult Surgeries()
        {
            var surgery = _context.Surgeries.ToList();
            ViewBag.Surgery = surgery;

            return View();
        }

        public IActionResult NewPrescription()
        {
            return View();
        }

        public IActionResult EditPrescription()
        {
            return View();
        }

        public IActionResult NewSurgery()
        {
            return View();
        }

        public IActionResult PatientRecord()
        {
            return View();
        }

        public IActionResult DischargePatient()
        {
            var discharge = _context.Patients.Include(c => c.Status == "Discharged").ToList();
            ViewBag.Patient = discharge;

            return View();
        }

        public IActionResult ConfirmTreatmentCodes()
        {
            return View();
        }
    }
}
