using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class OrderViewModel
    {
        [Required]
        public int OrderID { get; set; }


        public DateTime Date { get; set; }

        public string Status { get; set; }
        public string Medication { get; set; }
        public string Quantity { get; set; }

        public string Patient { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        [Required]
        public int PatientID { get; set; }
        [Required]
        public int SurgeryID { get; set; }
        [Required]
        public int MedicationID { get; set; }

        [Required]
        
        public int AnaesthesiologistID { get; set; }
        public List<OrderViewModel> Orders { get; set; }
    }
}
