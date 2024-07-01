using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Medication_Interaction
    {

        [ForeignKey("ICD_10_Code")]
        public int ICD_ID { get; set; }

        [ForeignKey("Active_Ingredient")]
        public int Active_IngredientID { get; set; }

        public virtual ICDCodes? ICDCodes { get; set; }
        public virtual Active_Ingredient? Active_Ingredient { get; set; }
    }
}
