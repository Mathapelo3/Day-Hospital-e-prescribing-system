using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class DisplayVitalsVM
    {
        public int PatientID { get; set; }
        public Patient? Patient { get; set; }

        public Patient_Vitals Patient_Vitals { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }


        public List<VitalsViewM> Vitals { get; set; }
        //public List<Patient_Vitals> Patient_Vitals { get; set; } = new List<Patient_Vitals>();


        //public DisplayVitalsVM()
        //{
           
        //    Vitals = new List<VitalsVM>();
        //    Patient_Vitals = new List<Patient_Vitals>();
        //}

        public class VitalsViewM
        {
            public DateTime Date { get; set; }
            public TimeSpan Time { get; set; }
           
            public string Vital { get; set; }
            public string Value { get; set; }
            public string Notes { get; set; }
        }
    }
}
