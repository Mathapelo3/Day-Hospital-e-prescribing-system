using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Ward")] // Specify the table name explicitly if it differs from the default
    public class Ward
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WardId { get; set; }

        [Required]
        [StringLength(50)]
        public string WardName { get; set; }

        [Required]
        [StringLength(50)]
        public string NumberOfBeds { get; set; }
    }
}
