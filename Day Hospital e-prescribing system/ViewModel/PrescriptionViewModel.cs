using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PrescriptionViewModel
    {
        public PrescriptionViewModel()
        {
            PatientList = new List<SelectListItem>();
            MedicationList = new List<SelectListItem>();
            SelectedMedications = new List<DayHospitalMedicationViewModel>();
        }

        [Required]
        public int PrescriptionID { get; set; }
        public string Medication { get; set; }
        public string Instruction { get; set; }
        public DateTime Date { get; set; }
        public string Quantity { get; set; }
        public string Status { get; set; }
        public bool Urgency { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public string Surgeon { get; set; }
        [Required]
        public int PatientID { get; set; }
        [Required]
        public int SurgeryID { get; set; }
        [Required]
        public int MedicationID { get; set; }

        public List<PrescriptionViewModel> Prescriptions { get; set; }

        public IEnumerable<SelectListItem> PatientList { get; set; }
        public IEnumerable<SelectListItem> MedicationList { get; set; }
        public int SelectedPatientId { get; set; }
        public List<DayHospitalMedicationViewModel> SelectedMedications { get; set; }

    }
}

