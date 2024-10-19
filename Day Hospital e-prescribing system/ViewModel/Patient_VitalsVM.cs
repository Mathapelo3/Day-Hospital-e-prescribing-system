namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class Patient_VitalsVM
    {
        public int Patient_VitalsID { get; set; }
        public int VitalsID { get; set; }
        public string? Vital { get; set; }
        public string? Value { get; set; }
        public string? Min { get; set; }
        public string? Max { get; set; }
        public string? Normal { get; set; }
        public DateTime Date { get; set; }
        public string? Height { get; set; }  
        public string? Weight { get; set; }
        public string? Time { get; set; }
        public string? Notes { get; set; }
        public string? Alert { get; set; }

    }
}
