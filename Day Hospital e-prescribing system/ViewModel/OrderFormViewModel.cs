using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class OrderFormViewModel
    {
        public int PatientID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }
        public bool Urgency { get; set; }

        public MultiSelectList? Medications { get; set; }

        //public string SelectedMedicationsJson { get; set; } = "[]";
        [Required(ErrorMessage = "Medications are required.")]

        public List<MedicationViewModel> SelectedMedications { get; set; } = new List<MedicationViewModel>();
    }
}
    

