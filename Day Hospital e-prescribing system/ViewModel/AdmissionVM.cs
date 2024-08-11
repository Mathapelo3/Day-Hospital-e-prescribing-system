using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication27.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdmissionVM
    {
        public int AdmissionID { get; set; }

        [Required]
        public int Surgery_TreatmentCodeID { get; set; }

        public int TreatmentCodeID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; }

        public int PatientID { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        public string BedName { get; set; }
        public string WardName { get; set; }
        public string Description { get; set; }

        [Required]
        public int? WardId { get; set; }

      
        [Required(ErrorMessage = "Please select a bed.")]
        public int BedId { get; set; }

        public int SurgeryID { get; set; }

        public virtual Bed Beds { get; set; }
        public bool IsAvailable { get; set; }

        public string TreatmentCode { get; set; }

        public virtual Patient Patients { get; set; }

        public virtual ICollection<Bed>? Bed { get; set; } = new List<Bed>();

        public IEnumerable<SelectListItem> Surgery_TreatmentCode { get; set; }



    }
}
