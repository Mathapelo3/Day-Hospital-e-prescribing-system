using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Day_Hospital_e_prescribing_system.Models
{
    public class DayHospitalRecords
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HospitalRecordID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string AddressLine1 { get; set; }

        [Required]
        [StringLength(50)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PMContactNo { get; set; }

        [Required]
        public int SuburbID { get; set; }

        // Navigation property
        [ForeignKey("SuburbID")]
        public virtual Suburb Suburb { get; set; }
    }
}
