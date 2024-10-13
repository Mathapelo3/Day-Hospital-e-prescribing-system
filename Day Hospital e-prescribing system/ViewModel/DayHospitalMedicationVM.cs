using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class DayHospitalMedicationVM
    {
        
        public int StockID {get;set;}
        
        public int QtyRecieved { get;set;}

        public int QtyUsed { get; set; }

        public int QtyLeft { get;set;}

        public int ReOrderLevel { get;set;}

        public string MedicationName { get;set;}

        public int Schedule {  get;set;}

        public int MedTypeId { get;set;}
        public string DosageForm { get; set; }

        public bool IsBelowReorderLevel { get; set; }

        public MedicationType MedicationType { get; set; }
        public List<MedicationType> MedicationTypes { get; set; }
        public List<Active_Ingredient> Active_Ingredients { get; set; }

        public string NewMedicationName { get; set; }
        public int NewMedTypeId { get; set; }
        public string NewDosageForm { get; set; }
        public int NewReOrderLevel { get; set; }
        public int NewQtyLeft { get; set; }
    }

}
