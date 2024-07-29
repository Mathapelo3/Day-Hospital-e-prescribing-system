using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Patient_Vitals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_VitalID { get; set; }

        [Required]
        [StringLength(50)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Time { get; set; }

        [Required]
        [StringLength(200)]
        public string Notes { get; set; }

        [Required]
        public int VitalID { get; set; }
        // Navigation property
        [ForeignKey("VitalsID")]
        public virtual Vitals Vitals { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patients { get; set; }
    }
}
