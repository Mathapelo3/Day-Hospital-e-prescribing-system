using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Day_Hospital_e_prescribing_system.Models
{
    public class Patient_Allergy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_AllergyID { get; set; }

        [Required]
        [ForeignKey("AllergyID")]
        public int AllergyID { get; set; }

        [Required]
        [ForeignKey("Active_IngredientID")]
        public int Active_IngredientID { get; set; }



        [Required]
        [ForeignKey("PatientID")]
        public int PatientID { get; set; }

        public virtual Active_Ingredient Active_Ingredient { get; set; }
        public virtual Allergy Allergy { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
