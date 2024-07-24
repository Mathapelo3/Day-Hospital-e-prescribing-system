using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Please enter patient name.")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter patient surname.")]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter patient date of birth.")]
        public DateTime DateOfBirth { get; set; }


        [Required(ErrorMessage = "Please enter patient ID Number.")]
        [StringLength(50)]
        public string IDNo { get; set; }

        [Required(ErrorMessage = "Please select patient gender.")]
        [StringLength(50)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter patient address.")]
        [StringLength(50)]
        public string AddressLine1 { get; set; }

        [Required(ErrorMessage = "Please enter patient address.")]
        [StringLength(50)]
        public string AddressLine2 { get; set; }

        [Required(ErrorMessage = "Please enter patient email.")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter patient cell number.")]
        [StringLength(10)]
        public string ContactNo { get; set; }

        [Required(ErrorMessage = "Please enter patient cell number.")]
        [StringLength(10)]
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
