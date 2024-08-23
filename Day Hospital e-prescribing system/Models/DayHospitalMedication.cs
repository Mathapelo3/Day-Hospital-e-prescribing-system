using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class DayHospitalMedication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockID { get; set; }

        [Required]
        [StringLength(50)]
        public string QtyRecived { get; set; }

        [Required]
        [StringLength(50)]
        public string QtyUsed { get; set; }

        [Required]
        [StringLength(50)]
        public string QtyLeft { get; set; }

        [Required]
        [StringLength(50)]
        public string ReOrderLevel { get; set; }

        [Required]
        [StringLength(40)]
        public string MedicationName { get; set; }

        [Required] 
        public int Schedule { get; set; }

        [Required]
        public int DosageForm { get; set; }
        // Navigation property
        [ForeignKey("MedTypeID")]
        public virtual MedicationType MedicationType { get; set; }

    }
}
