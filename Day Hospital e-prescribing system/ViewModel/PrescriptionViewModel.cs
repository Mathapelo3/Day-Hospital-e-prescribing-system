using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PrescriptionViewModel
    {
        [Key]
        public int PrescriptionID { get; set; }
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string? SurgeonName { get; set; } 
        public string? SurgeonSurname { get; set; } 
        public string? MedicationName { get; set; }
        public DateTime Date { get; set; }
        public string? InstructionText { get; set; } 
        public int? Quantity { get; set; } 
        public string? Status { get; set; } 
        public bool Urgency { get; set; }





    }

}

