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
        public DateTime Date { get; set; }

        [Required]

        public string? Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]

        public bool Urgency { get; set; }



        public bool? Administered { get; set; }



        public string? QAdministered { get; set; }



        public string? Notes { get; set; }


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
        public int MedicationID { get; set; }
        // Navigation property
        [ForeignKey("MedicationID")]
        public virtual Medication Medication { get; set; }
    }

}
