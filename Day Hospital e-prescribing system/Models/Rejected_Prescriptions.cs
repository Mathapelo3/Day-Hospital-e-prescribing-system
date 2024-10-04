using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Rejected_Prescriptions
    {
        [Key]
        public int Rejected_PrescriptionsId { get; set; }

        [Required]
        public int PharmacistId { get; set; }

        [Required]
        public int PrescriptionId { get; set; }

        public string Reason { get; set; }

        [ForeignKey("PharmacistId")]
        public virtual Pharmacist Pharmacist { get; set; }

        [ForeignKey("PrescriptionId")]
        public virtual Prescription Prescription { get; set; }
    }
}
