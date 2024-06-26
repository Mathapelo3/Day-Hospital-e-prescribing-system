using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactNo { get; set; }

        
        [Required]
        [StringLength(10)]
        public string HCRNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(50)]

        public string Password { get; set; }

       

        [Required]
        public int AdminID { get; set; }

        // Navigation property
        [ForeignKey("AdminID")]
        public virtual Admin Admin { get; set; }
        [Required]
        public int SpecializationID { get; set; }

        // Navigation property
        [ForeignKey("SpecializationID")]
        public virtual Specialization Specialization { get; set; }
    }
}
