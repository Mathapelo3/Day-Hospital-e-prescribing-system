using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ConditionsVM
    {

        public string IDNo { get; set; }
        public int PatientID { get; set; } // Assuming PatientID is passed to the ViewModel
        public string Name { get; set; }
        public string Surname { get; set; }
        public Patient Patient { get; set; }

        public int SurgeryID { get; set; }

        public int AllergyID { get; set; }
        public string Allergy_Name { get; set; }
        public IEnumerable<SelectListItem> Allergy { get; set; }


        public int ConditionID { get; set; }
        public string Condition_Name { get; set; }
        public IEnumerable<SelectListItem> Condition { get; set; }


        public int General_MedicationID { get; set; }
        public string Meds_Name { get; set; }
        public IEnumerable<SelectListItem> General_Medication { get; set; }

        //admision

        public List<string> Allergies { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> General_Medications { get; set; }

        public string SelectedCondition { get; set; }
        //public List<SelectListItem> Conditions { get; set; }


        public string SelectedAllergy { get; set; }
        //public List<SelectListItem> Allergies { get; set; }


        public string SelectedMedication { get; set; }
       
    }
}
