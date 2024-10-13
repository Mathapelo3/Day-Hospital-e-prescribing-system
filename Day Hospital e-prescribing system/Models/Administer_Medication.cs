using Day_Hospital_e_prescribing_system.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Administer_Medication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Administer_MedicationID { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [StringLength(50)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required.")]
        [StringLength(50)]
        public TimeSpan Time { get; set; }

        public string Administer { get; set; }

        [Required]
        public int PrescriptionID { get; set; }

        // Navigation property
        [ForeignKey("PrescriptionID")]
        public virtual Prescription Prescription { get; set; }

        [Required]
        public int NurseID { get; set; }

        // Navigation property
        [ForeignKey("NurseID")]
        public virtual Nurse Nurse { get; set; }
    }
}
