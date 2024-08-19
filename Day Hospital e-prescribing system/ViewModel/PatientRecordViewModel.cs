using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientRecordViewModel
    {
        public Patient Patient { get; set; }
        public List<Allergy> Allergies { get; set; }
        public List<Vitals> Vitals { get; set; }

        // Constructor to initialize lists
        public PatientRecordViewModel()
        {
            Allergies = new List<Allergy>();
            Vitals = new List<Vitals>();
        }
    }

}
