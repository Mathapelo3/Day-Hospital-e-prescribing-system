namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PrescriptionVM
    {
        public int PrescriptionID { get; set; }
        public string Instruction { get; set; }
        public DateTime Date { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        public bool Urgency { get; set; }
        public string MedicationName { get; set; }
        public string SurgeonName { get; set; }
        public string PatientName { get; set; }
        public int SurgeonID { get; set; }
        public int PatientID { get; set; }
        public int MedicationID { get; set; }
        public string Medication { get; set; }
    }
}
