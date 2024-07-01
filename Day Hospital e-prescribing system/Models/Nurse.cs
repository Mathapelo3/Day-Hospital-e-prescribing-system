using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Day_Hospital_e_prescribing_system.Models
{
    public class Nurse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NurseID { get; set; }

        [Required]
        public int UserID { get; set; }

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
