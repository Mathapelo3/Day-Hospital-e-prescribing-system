namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class UpdateMedicationQuantityVM
    {
        public int StockID { get; set; }
        public int Quantity { get; set; }

        public bool IsStockUpdated { get; set; } = false;
    }
}
