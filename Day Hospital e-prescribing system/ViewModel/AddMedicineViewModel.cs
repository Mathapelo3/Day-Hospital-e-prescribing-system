namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AddMedicineViewModel
    {
        public int MedTypeId { get; set; }
        public string MedicationName { get; set; }
        public int Schedule { get; set; }
        public int ReOrderLevel { get; set; }

        // Add a list to hold active ingredients
        public List<ActiveIngredientViewModel> ActiveIngredients { get; set; }
    }
}
