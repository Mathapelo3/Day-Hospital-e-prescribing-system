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
        public string Status { get; set; }
    }
}
