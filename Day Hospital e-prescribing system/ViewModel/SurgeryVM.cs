namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class SurgeryVM
    {
        public int SurgeryID { get; set; }
        public bool? Administered { get; set; }   // Nullable boolean to handle NULL values
        public int? AnaesthesiologistID { get; set; }   // Nullable if not always required
        public DateTime? Date { get; set; }  // Nullable to handle cases where Date is missing
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string? QAdministered { get; set; }   // Nullable for quality administered
        public int? SurgeonID { get; set; }   // Nullable for optional Surgeon data
        public int? TheatreID { get; set; }   // Nullable Theatre ID
        public TimeSpan? Time { get; set; }   // Nullable for time
        public bool? Urgency { get; set; }   // String because it may be descriptive
    }
}
