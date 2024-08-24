namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AddMedicineViewModel
    {
        public int DosageForm { get; set; }
        public string MedicineName { get; set; }
        public int Schedule { get;set; }
        public string ReOrderlevel { get; set; }
        
        public string ActiveIngredientIndication { get; set; }
        public string Strength { get; set; }
    }
}
