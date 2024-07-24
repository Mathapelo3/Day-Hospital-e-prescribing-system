using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Patient_Medication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_MedicationID { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patients { get; set; }

        [Required]
        public int General_MedicationID { get; set; }
        // Navigation property
        [ForeignKey("General_MedicationID")]
        public virtual General_Medication General_Medications { get; set; }
    }
}
