using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ConditionsVM
    {
        public int ConditionID { get; set; }

        [Required]
        [StringLength(50)]
        public string Condition_Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Condition_Description { get; set; }

        public int AllergyID { get; set; }

        [Required]
        [StringLength(50)]
        public string Allergy_Name { get; set; }

        public int General_MedicationID { get; set; }

        [Required]
        [StringLength(50)]
        public string Meds_Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Meds_Descriptions { get; set; }

        public IEnumerable<SelectListItem> Condition { get; set; }
        public IEnumerable<SelectListItem> Allergy { get; set; }

        public IEnumerable<SelectListItem> General_Medication { get; set; }
    }
}
