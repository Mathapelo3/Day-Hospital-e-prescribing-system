namespace Day_Hospital_e_prescribing_system.Models
{
    public class RejectedPrescriptions
    {
        public int Id { get; set; }
        public int PrescriptionId { get; set; }
        public string Reason { get; set; }
    }
}
