using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientRecordViewModel
    {
        public string IDNo { get; set; }
        public Patient? Patient { get; set; }
        public List<Allergy> Allergies { get; set; } = new List<Allergy>();
        public List<Patient_Vitals> Patient_Vitals { get; set; } = new List<Patient_Vitals>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        public PatientRecordViewModel()
        {
            Allergies = new List<Allergy>();
            Patient_Vitals = new List<Patient_Vitals>();
            Conditions = new List<Condition>();
        }
    }

}
