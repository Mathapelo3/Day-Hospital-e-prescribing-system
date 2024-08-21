using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }

        public string Patient { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string WardName { get; set; }
        public string BedName { get; set; }
        public string Nurse { get; set; }

        public string Surgeon { get; set; }
        public string Status { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }

        public List<VitalsViewModel> Vitals { get; set; }
        public List<string> Allergies { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> Medications { get; set; }
    }
}
