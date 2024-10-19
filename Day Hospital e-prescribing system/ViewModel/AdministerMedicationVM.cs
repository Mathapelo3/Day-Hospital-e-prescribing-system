namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdministerMedicationVM
    {
        public int PatientID { get; set; }
        public int PrescriptionID { get; set; }
        public int NurseID { get; set; }
        public string? Medication { get; set; }
        public string? Quantity { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Administer { get; set; }

       
    }
}
