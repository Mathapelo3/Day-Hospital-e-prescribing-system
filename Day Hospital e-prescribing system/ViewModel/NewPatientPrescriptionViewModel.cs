using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class NewPatientPrescriptionViewModel
    {
        public NewPatientPrescriptionViewModel()
        {
            PatientList = new List<SelectListItem>();
            SurgeonList = new List<SelectListItem>();
            MedicationList = new List<SelectListItem>();
            SelectedMedications = new List<MedicationItem>();
            Date = DateTime.Today;
        }

        [Required(ErrorMessage = "Please select a patient")]
        [Display(Name = "Patient")]
        public int SelectedPatientId { get; set; }

        [Required(ErrorMessage = "Please select a surgeon")]
        [Display(Name = "Surgeon")]
        public int SelectedSurgeonId { get; set; }

        [Required(ErrorMessage = "Please enter a date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Display(Name = "Urgent")]
        public bool Urgency { get; set; }

        [Display(Name = "Selected Medications")]
        [MinLength(1, ErrorMessage = "Please select at least one medication")]
        public List<MedicationItem> SelectedMedications { get; set; }

        [Display(Name = "Available Patients")]
        public IEnumerable<SelectListItem> PatientList { get; set; }

        [Display(Name = "Available Surgeons")]
        public IEnumerable<SelectListItem> SurgeonList { get; set; }

        [Display(Name = "Available Medications")]
        public IEnumerable<SelectListItem> MedicationList { get; set; }

        //[Display(Name = "Patient Name")]
        //public string PatientName { get; set; }

        //[Display(Name = "Surgeon Name")]
        //public string SurgeonName { get; set; }

        //public int SurgeonId { get; set; }

        public class MedicationItem
        {
            public int StockID { get; set; }

            [Required(ErrorMessage = "Medication name is required")]
            [Display(Name = "Medication Name")]
            public string MedicationName { get; set; }

            [Required(ErrorMessage = "Quantity is required")]
            [RegularExpression(@"^\d+$", ErrorMessage = "Quantity must be a positive integer")]
            public string Quantity { get; set; }

            [Required(ErrorMessage = "Instruction is required")]
            [StringLength(500, ErrorMessage = "Instruction cannot be longer than 500 characters")]
            public string Instruction { get; set; }
        }

        public void AddMedication(MedicationItem item)
        {
            SelectedMedications.Add(item);
        }

        public void RemoveMedication(int medID)
        {
            SelectedMedications.RemoveAll(m => m.StockID == medID);
        }

        public void ClearMedications()
        {
            SelectedMedications.Clear();
        }
    }
}