using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Surgery")]
    public class Surgery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SurgeryID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(20)]
        public string Time { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        public bool Urgency { get; set; }

        [Required]
        public bool Administered { get; set; }

        [Required]
        [StringLength(200)]
        public string QAdministered { get; set; }

        [Required]
        [StringLength(200)]
        public string Notes { get; set; }

        [Required]
        public int Surgery_TreatmentCodeID { get; set; }
        // Navigation property
        [ForeignKey("Surgery_TreatmentCodeID")]
        public virtual Surgery_TreatmentCode Surgery_TreatmentCodes { get; set; }

        [Required]
        public int TheatreID { get; set; }
        // Navigation property
        [ForeignKey("TheatreID")]
        public virtual Theatre Theatres { get; set; }

        [Required]
        public int AnaesthesiologistID { get; set; }
        // Navigation property
        [ForeignKey("AnaesthesiologistID")]
        public virtual Anaesthesiologist Anaesthesiologists { get; set; }

        

        [Required]
        public int SurgeonID { get; set; }
        // Navigation property
        [ForeignKey("SurgeonID")]
        public virtual Surgeon Surgeons { get; set; }

        [Required]
        public int NurseID { get; set; }
        // Navigation property
        [ForeignKey("NurseID")]
        public virtual Nurse Nurses { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patients { get; set; }
    }
}
