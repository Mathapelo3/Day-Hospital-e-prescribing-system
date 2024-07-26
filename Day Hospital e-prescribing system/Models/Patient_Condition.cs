using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Patient_Condition")]
    public class Patient_Condition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_ConditionID { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }

        [Required]
        public int ConditionID { get; set; }
        // Navigation property
        [ForeignKey("ConditionID")]
        public virtual Condition Condition { get; set; }
    }
}
