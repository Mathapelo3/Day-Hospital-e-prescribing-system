using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Vitals
    {
        [Key]
        
        public int VitalsID { get; set; }

       


        [Required]
        [StringLength(50)]
        public string Vital { get; set; }

        [Required]
        [StringLength(50)]
        public string Min { get; set; }
        [Required]
        [StringLength(50)]
        public string Max { get; set; }

        [StringLength(100)]
        public string? Height { get; set; }

        [StringLength(100)]
        public string? Weight { get; set; }
    }
}
