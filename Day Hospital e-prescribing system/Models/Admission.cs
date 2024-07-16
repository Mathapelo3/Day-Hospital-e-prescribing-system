using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Admission")]
    public class Admission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdmissionID { get; set; }

        [Required]
        [StringLength(50)]
       public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Time { get; set; }

        [Required]
        public int SurgeonID { get; set; }

        // Navigation property
        [ForeignKey("SurgeonID")]
        public virtual Surgeon Surgeons { get; set; }

        [Required]
        public int AnaesthesiologistID { get; set; }

        // Navigation property
        [ForeignKey("AnaesthesiologistID")]
        public virtual Anaesthesiologist Anaesthesiologists { get; set; }

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
