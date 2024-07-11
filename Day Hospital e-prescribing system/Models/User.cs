using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    
    public class User
    {
        [Key]
        
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

        public string HashedPassword { get; set; }

        public int RoleId { get; set; }

        [Required]
        public int AdminID { get; set; }

        // Navigation property
        [ForeignKey("AdminID")]
        public virtual Admin Admins { get; set; }


        [Required]
        public int RoleId { get; set; }

        // Navigation property
        [ForeignKey("RoleId")]

        public virtual Role Role { get; set; }

    }
}
