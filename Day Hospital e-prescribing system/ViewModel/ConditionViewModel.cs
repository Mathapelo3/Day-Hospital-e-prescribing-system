using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ConditionViewModel
    {
        public int ConditionID { get; set; }

        [Required(ErrorMessage = "Please add ICD Code.")]
        [StringLength(255)]
        public string ICD_10_Code { get; set; }

        [Required(ErrorMessage = "Please add condition name.")]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
