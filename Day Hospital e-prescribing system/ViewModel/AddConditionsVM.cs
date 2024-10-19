using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AddConditionsVM
    {
        public int PatientID { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Please select a condition")]
        public string? SelectedCondition { get; set; }

        [Required(ErrorMessage = "Please select an allergy")]
        public string? SelectedAllergy { get; set; }

        [Required(ErrorMessage = "Please select a medication")]
        public string? SelectedMedication { get; set; }

        public IEnumerable<SelectListItem>? Condition { get; set; }
        public IEnumerable<SelectListItem>? Active_Ingredient{ get; set; }
        public IEnumerable<SelectListItem>? General_Medication { get; set; }

        public List<string>? Conditions { get; set; }
        public List<string>? Active_Ingredients { get; set; }
        public List<string>? General_Medications { get; set; }
    }
}
