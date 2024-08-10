using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication27.Models
{
    [Table("Bed")]
    public class Bed
    {
        [Key]
        public int BedId { get; set; }

        [Required]
        public string BedName { get; set; }

        public bool IsAvailable { get; set; }

        public int WardId { get; set; }

        [ForeignKey("WardId")]
        public virtual Ward Ward { get; set; }
    }
}
