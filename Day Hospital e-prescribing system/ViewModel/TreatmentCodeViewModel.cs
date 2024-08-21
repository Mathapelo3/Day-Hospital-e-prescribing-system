using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class TreatmentCodeViewModel
    {
        [Key]
        public int TreatmentCodeID { get; set; }

        public string Description { get; set; }

        public string ICD_10_Code { get; set; }

        public int Surgery_TreatmentCodeID { get; set; }
    }
}
