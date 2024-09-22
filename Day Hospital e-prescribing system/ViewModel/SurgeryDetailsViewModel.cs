using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class SurgeryDetailsViewModel
    {
        public int SurgeryID { get; set; }
        public int PatientID { get; set; }
        public int AnaesthesiologistID { get; set; }
        public int TheatreID { get; set; }
        public string? PatientName { get; set; }
        public string? PatientSurname { get; set; }
        public string? SurgeryCode { get; set; } // Mark as nullable
        public string? TheatreName { get; set; }
        public string? AnaesthesiologistName { get; set; }
        public string? AnaesthesiologistSurname { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }

        [NotMapped]
        public List<string> SurgeryCodes { get; set; } = new List<string>(); // Initialize to empty list
    }
}
