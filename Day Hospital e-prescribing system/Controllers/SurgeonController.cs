using Microsoft.AspNetCore.Mvc;

namespace Day_Hospital_e_prescribing_system.Controllers
{
    public class SurgeonController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Prescriptions()
        {
            return View();
        }

        public IActionResult Patients()
        {
            return View();
        }

        public IActionResult Surgeries()
        {
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

        public IActionResult BookedPatients()
        {
            return View();
        }

        public IActionResult AdmissionRecord()
        {
            return View();
        }

        public IActionResult DischargePatient()
        {
            return View();
        }
    }
}
