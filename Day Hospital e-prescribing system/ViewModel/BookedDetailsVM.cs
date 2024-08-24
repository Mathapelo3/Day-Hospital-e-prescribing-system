namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class BookedDetailsVM
    {
        public int AdmissionID { get; set; }
        public string IDNo { get; set; }
        public int SurgeonID { get; set; }
        public int SurgeryID { get; set; }
        public int? PatientID { get; set; }
        public int? AnaesthesiologistID { get; set; }
        public int? TreatmentCodeID { get; set; }
        public int? TheatreID { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string ICD_Code_10 { get; set; }
        public string TheatreName { get; set; }
        public string AnaesthesiologistName { get; set; }
        public string AnaesthesiologistSurname { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string Gender { get; set; }
        public string SurgeonName { get; set; }
        public string SurgeonSurname { get; set; }
    }
}
