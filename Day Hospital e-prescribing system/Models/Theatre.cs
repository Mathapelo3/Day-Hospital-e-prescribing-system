using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Theatre")] // Specify the table name explicitly if it differs from the default
    public class Theatre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TheatreID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
