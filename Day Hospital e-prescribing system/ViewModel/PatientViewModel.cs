using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }

        public string Patient { get; set; }

        public DateTime Date { get; set; }
        public string Time { get; set; }

        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Nurse { get; set; }
<<<<<<< HEAD
        public string Status { get; set; }
=======

        public string Height { get; set; }
        public string Weight { get; set; }

        public List<VitalsViewModel> Vitals { get; set; }
        public List<string> Allergies { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> Medications { get; set; }
>>>>>>> 0bc33f67d25189d703a3ea32bc31ca09b68a6646
    }
}
