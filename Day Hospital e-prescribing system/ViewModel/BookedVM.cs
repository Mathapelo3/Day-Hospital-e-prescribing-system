namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class BookedVM
    {

        public int IDNo { get; set; }
        public int SurgeryID { get; set; }
        public int PatientID { get; set; }
        public string Patient { get; set; }

        public DateTime Date { get; set; }
        public string Time { get; set; }

        public string Surgery_TreatmentCode { get; set; }
        public string WardName { get; set; }
        public string BedName { get; set; }
        public string Anaesthesiologist { get; set; }
        public string Theatre { get; set; }

        public string Surgeon { get; set; }
    }
}
