using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel.PharmacistViewModel
{
    public class DispensedPrescription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PrescriptionID { get; set; }

        [Required]
        [StringLength(100)]
        public string Instruction { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public bool Urgency { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patients { get; set; }

        [Required]
        public int SurgeonID { get; set; }
        // Navigation property
        [ForeignKey("SurgeonID")]
        public virtual Surgeon Surgeons { get; set; }

        [Required]
        public int PharmacistID { get; set; }
        // Navigation property
        [ForeignKey("PharmacistID")]
        public virtual Pharmacist Pharmacists { get; set; }

        [Required]
        public int MedicationID { get; set; }
        // Navigation property
        [ForeignKey("MedicationID")]
        public virtual Medication Medications { get; set; }

        public string Patient { get; set; }
        public string Surgeon { get; set; }
        public string Medication { get; set; }
        public string Pharmacist { get; set; }

    }
}
