using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Pharmacist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PharmacistID { get; set; }

        [Required]
        public int UserID { get; set; }

        //public string Username { get; set; }

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public ICollection<Rejected_Prescriptions> Prescriptions { get; set; } = new HashSet<Rejected_Prescriptions>();
    }
}
