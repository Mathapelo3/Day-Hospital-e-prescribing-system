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

        // Represents the name of the allergy
        public string AllergyName { get; set; }

        // Represents the description of the active ingredient
        public string Active_IngredientDescription { get; set; }

        // Additional properties for the procedure logic
        public bool ShouldTriggerAlert { get; set; } = false;

        // Optional: Property to hold Medication IDs if needed
        public List<int> MedicationIDs { get; set; } = new List<int>();
    }

}
