using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientAllergiesViewModel
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

        public string AllergyName { get; set; }

        public string Active_IngredientDescription { get; set; }
    }
}
