using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ConditionViewModel
    {
        public int ConditionID { get; set; }

        [Required(ErrorMessage = "The Description field is required.")]
        [StringLength(255)]
        public string ICD_10_Code { get; set; }

        [Required(ErrorMessage = "The Name field is required.")]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
