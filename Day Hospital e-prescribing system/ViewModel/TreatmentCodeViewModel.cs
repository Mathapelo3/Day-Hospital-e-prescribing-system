using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class TreatmentCodeViewModel
    {
        public int TreatmentCodeID { get; set; }

        [Required(ErrorMessage = "Please add the ICD Code.")]
        [StringLength(10)]
        public string ICD_10_Code { get; set; }

        [Required(ErrorMessage = "Please add the description.")]
        [StringLength(100)]
        public string Description { get; set; }
    }
}
