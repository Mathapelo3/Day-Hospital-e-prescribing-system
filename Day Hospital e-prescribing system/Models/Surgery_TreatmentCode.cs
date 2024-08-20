using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Surgery_TreatmentCode")]
    public class Surgery_TreatmentCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Surgery_TreatmentCodeID { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string ICD_10_Code { get; set; }
    }
}
