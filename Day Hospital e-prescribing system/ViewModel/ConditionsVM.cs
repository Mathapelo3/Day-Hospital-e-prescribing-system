using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ConditionsVM
    {

        public int PatientID { get; set; } // Assuming PatientID is passed to the ViewModel
        public string Name { get; set; }
        public string Surname { get; set; }

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
        public int AdmissionID { get; set; } // Assuming PatientID is passed to the ViewModel
        public List<Allergy> Allergies { get; set; }
        public List<Condition> Conditions { get; set; }
        public List<General_Medication> Medications { get; set; }

        public virtual ICollection<Patient_Allergy> Patient_Allergy { get; set; }
        public virtual ICollection<Patient_Condition> Patient_Condition { get; set; }
        public virtual ICollection<Patient_Medication> Patient_Medication { get; set; }


    }
}
