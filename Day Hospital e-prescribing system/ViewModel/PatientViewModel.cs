using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientID { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string DateOfBirth { get; set; }

        public string IDNo { get; set; }

        public string Gender { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string Email { get; set; }

        public string ContactNo { get; set; }

        public string NextOfKinNo { get; set; }

        public string Status { get; set; }

        [Required]
        public int WardID { get; set; }
        // Navigation property
        [ForeignKey("WardID")]
        public virtual Ward Wards { get; set; }

        public int TreatmentCode { get; set; }

        [Required]
        public int SuburbID { get; set; }
        // Navigation property
        [ForeignKey("SuburbID")]
        public virtual Suburb Suburbs { get; set; }

        public string Patient { get; set; }

        public DateTime Date { get; set; }
        public string Time { get; set; }

        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Nurse { get; set; }
    }
}
