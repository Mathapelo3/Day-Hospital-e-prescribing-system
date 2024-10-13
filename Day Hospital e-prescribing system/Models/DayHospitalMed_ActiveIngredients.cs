using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class DayHospitalMed_ActiveIngredients
    {
        [Key]
        public int DayHospitalMed_ActiveIngredientID {  get; set; }

        public int Active_IngredientID { get; set; }

        public int StockID { get; set; }

        public string Strenght { get; set; }

        public Active_Ingredient Active_Ingredient { get; set; }
        public DayHospitalMedication DayHospitalMedication { get; set; }
        
    }
}
