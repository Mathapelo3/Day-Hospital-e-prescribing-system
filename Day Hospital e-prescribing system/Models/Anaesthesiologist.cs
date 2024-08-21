using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Anaesthesiologist")]
    public class Anaesthesiologist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnaesthesiologistID { get; set; }

        [Required]
        public int UserID { get; set; }

        

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
