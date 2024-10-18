using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class SurgeryViewModel
    {
        [Required(ErrorMessage = "Please select a date.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please select a time slot.")]
        [DisplayName("Time Slot")]
        public string Time { get; set; }

        [Required(ErrorMessage = "Please select a patient.")]
        [DisplayName("Patient")]
        public int PatientID { get; set; }

        [DisplayName("Surgeon")]
        public int SurgeonID { get; set; }

        [Required(ErrorMessage = "Please select an anaesthesiologist.")]
        [DisplayName("Anaesthesiologist")]
        public int AnaesthesiologistID { get; set; }

        [Required(ErrorMessage = "Please select a theatre")]
        [DisplayName("Theatre")]
        public int TheatreID { get; set; }

        [Required(ErrorMessage = "Please select at least one treatment code.")]
        [DisplayName("Treatment Codes")]
        public List<string> SelectedTreatmentCodes { get; set; }

        public List<SelectListItem> PatientList { get; set; }
        public List<SelectListItem> SurgeonList { get; set; }
        public List<SelectListItem> AnaesthesiologistList { get; set; }
        public List<SelectListItem> TheatreList { get; set; }
        public List<SelectListItem> TreatmentCodeList { get; set; }

        // Added to display surgeon information
        public string SurgeonName { get; set; }
    }
}
