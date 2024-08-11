using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Patient")]
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientID { get; set; }

        [Required]
        [StringLength(50)]
        public string? Name { get; set; }

        [Required]
        [StringLength(50)]
        public string? Surname { get; set; }

        [Required]
        [StringLength(50)]
        public string? DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string? IDNo { get; set; }

        [Required]
        [StringLength(50)]
        public string? Gender { get; set; }

        [Required]
        [StringLength(50)]
        public string? AddressLine1 { get; set; }

        [Required]
        [StringLength(50)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(50)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string? ContactNo { get; set; }

        [Required]
        [StringLength(20)]
        public string? NextOfKinNo { get; set; }

        [Required]
        [StringLength(50)]
        public string? Status { get; set; }

        [Required]
        public int WardId { get; set; }
        // Navigation property
        [ForeignKey("WardId")]
        public virtual Ward Wards { get; set; }

        [Required]
        public int? TreatmentCodeID { get; set; }
        // Navigation property
        [ForeignKey("TreatmentCodeID")]
        public virtual TreatmentCode TreatmentCodes { get; set; }

        [Required]
        public int? SuburbID { get; set; }
        // Navigation property
        [ForeignKey("SuburbID")]
        public virtual Suburb Suburbs { get; set; }


        public virtual ICollection<Patient_Allergy> Patient_Allergy { get; set; }
        public virtual ICollection<Patient_Condition> Patient_Condition { get; set; }
        public virtual ICollection<Patient_Medication> Patient_Medication { get; set; }
    }
}
