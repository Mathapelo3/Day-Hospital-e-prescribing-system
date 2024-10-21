using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class DispenseReportDataViewModel
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Date { get; set; }

        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string SurgeonName { get; set; }
        public string MedicationName { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        
    }
}
