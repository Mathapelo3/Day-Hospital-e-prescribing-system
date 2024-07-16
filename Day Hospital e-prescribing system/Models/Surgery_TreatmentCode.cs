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
        [StringLength(50)]
        public string Description { get; set; }
    }
}
