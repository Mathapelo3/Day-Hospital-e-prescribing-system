namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PPrescriptionViewModel
    {
        public int PrescriptionID { get; set; }
        public string Instruction { get; set; }
        public DateTime Date { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        public bool Urgency { get; set; }
        public string MedicationName { get; set; }
        public string SurgeonFullName { get; set; }
        public string PatientFullName { get; set; }

        public int PatientID { get; set; }

    }
}
