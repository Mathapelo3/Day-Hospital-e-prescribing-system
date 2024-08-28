using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PostSurgeryOrderViewModel
    {
        public int OrderID { get; set; }
        public DateTime Date { get; set; }
        public string MedicationName { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        public bool Urgency { get; set; }
        public bool Administered { get; set; }
        public string? QAdministered { get; set; }
        public string? Notes { get; set; }
    }
}
