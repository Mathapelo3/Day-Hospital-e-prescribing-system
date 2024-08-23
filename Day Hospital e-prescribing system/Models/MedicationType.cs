using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class MedicationType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }
    }
}
