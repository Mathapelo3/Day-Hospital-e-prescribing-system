using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class EditVitalsViewModel
    {
        public int VitalsID { get; set; }

        [Required(ErrorMessage = "The Vital field is required.")]
        [StringLength(50)]
        public string Vital { get; set; }

        
        [StringLength(50)]
        public string? Min { get; set; }

        
        [StringLength(50)]
        public string? Max { get; set; }

        
        [StringLength(50)]
        public string? Normal { get; set; }

        public bool IsEditing { get; set; }
    }
}
