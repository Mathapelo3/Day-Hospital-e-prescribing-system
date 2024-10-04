using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Prescription_Medications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PrescriptionID { get; set; }

        [Required]
        public int StockID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string InstructionText { get; set; }

        public virtual Prescription Prescription { get; set; }
        public virtual DayHospitalMedication DayHospitalMedication { get; set; }
    }
}
