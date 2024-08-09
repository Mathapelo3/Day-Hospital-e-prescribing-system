using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Surgeon")]
    public class Surgeon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SurgeonID { get; set; }

        [Required]
        public int UserID { get; set; }

        public string Username { get; set; }

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
