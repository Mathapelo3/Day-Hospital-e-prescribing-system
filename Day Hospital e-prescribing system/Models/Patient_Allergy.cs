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
        public int AllergyID { get; set; }
        // Navigation property
        [ForeignKey("AllergyID")]
        public virtual Allergy Allergy { get; set; }
        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }
    }
}
