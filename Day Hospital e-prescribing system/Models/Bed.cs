using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Bed")]
    public class Bed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BedId { get; set; }

        [Required]
        [StringLength(50)]
        public string BedName { get; set; }

        [Required]
        [StringLength(50)]
        public string isAvaible { get; set; }

        [Required]
        public int WardId { get; set; }

        // Navigation property
        [ForeignKey("WardId")]
        public virtual Ward Wards { get; set; }
    }
}
