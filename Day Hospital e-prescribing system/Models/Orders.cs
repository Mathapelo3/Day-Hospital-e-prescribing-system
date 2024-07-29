using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Orders
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [StringLength(50)]
        public string Urgency { get; set; }

        [Required]
        [StringLength(50)]
        public string Administered { get; set; }

        [Required]
        [StringLength(50)]
        public string QAdministered { get; set; }

        [Required]
        [StringLength(200)]
        public string Notes { get; set; }


        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }

        [Required]
        public int AnaesthesiologistID { get; set; }
        // Navigation property
        [ForeignKey("AnaesthesiologistID")]
        public virtual Anaesthesiologist Anaesthesiologist { get; set; }
        [Required]
        public int SurgeonID { get; set; }
        // Navigation property
        [ForeignKey("SurgeonID")]
        public virtual Surgeon Surgeon { get; set; }

        [Required]
        public int MedicationID { get; set; }
        // Navigation property
        [ForeignKey("MedicationID")]
        public virtual Medication Medications { get; set; }
    }

}
