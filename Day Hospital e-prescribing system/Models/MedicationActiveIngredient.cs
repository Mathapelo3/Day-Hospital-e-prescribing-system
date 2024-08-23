using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class MedicationActiveIngredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Medication_Active_IngredientID { get; set; }

        [Required]
        public int MedicationActiveIngredientID { get; set; }
        [ForeignKey("Active_Ingredient_InteractionID")]
        public virtual Active_Ingredient Active_Ingredient { get; set; }

        [Required]
        public int MedicationId { get; set; }
        [ForeignKey("MedicationID")]
        public virtual Medication Medication { get; set; }

        public string Strength { get; set; }

        [Required]
        public int StockID { get; set; }
        [ForeignKey("StockID")]
        public virtual DayHospitalMedication DayHospitalMedication { get; set; }

    }
}
