namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class OrderMedicineViewModel
    {
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public int StockID { get; set; }
        public string MedicationName { get; set; }
        public int MedTypeId { get; set; }
        public string DosageForm { get; set; }
        public int QtyLeft { get; set; }
        public int QtyReceived { get; set; }
        public bool Urgency { get; set; }
    }
}
