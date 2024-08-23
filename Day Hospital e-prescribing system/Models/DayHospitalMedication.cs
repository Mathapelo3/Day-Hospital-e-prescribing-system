using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("DayHospitalMedication")]
    public class DayHospitalMedication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StockID { get; set; }
        public string QtyReceived { get; set; }
        public string QtyUsed { get; set; }
        public string QtyLeft { get; set; }
        public string ReOrderLevel { get; set; }
        public string MedicationName { get; set; }
        public int Schedule {  get; set; }
        public int DosageForm {  get; set; }
    }
}
