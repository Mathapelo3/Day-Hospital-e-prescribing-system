using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class VitalsVM
    {
        public int SurgeryID { get; set; }
        public int PatientID { get; set; }
        public int VitalsID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Notes { get; set; }
        public int AdmissionID { get; set; }
        public List<Patient_VitalsVM> Vitals { get; set; }
        public Patient Patient { get; set; }

    }
}
