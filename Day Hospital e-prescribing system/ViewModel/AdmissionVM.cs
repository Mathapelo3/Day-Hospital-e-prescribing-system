using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication27.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdmissionVM
    {

        public int AdmissionID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string? Time { get; set; }

        public int PatientID { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }



        public string? BedName { get; set; }
        public string? WardName { get; set; }
        public string? Description { get; set; }

        //[Required(ErrorMessage = "Ward is required")]
        public int WardId { get; set; }

        //[Required(ErrorMessage = "Bed is required")]
        public int BedId { get; set; }

        public int SurgeryID { get; set; }

        public string? SurgeonName { get; set; }
        public string? SurgeonSurname { get; set; }

        //public virtual Bed Beds { get; set; }
        public bool  IsAvailable { get; set; }

        //public string TreatmentCode { get; set; }
        public string? ICD_10_Code { get; set; }

        public virtual Patient? Patients { get; set; }

        public virtual ICollection<Bed>? Bed { get; set; } = new List<Bed>();

        [BindNever]
        public IEnumerable<SelectListItem>? Beds { get; set; }

        // List of wards for the dropdown
        public List<SelectListItem>? WardList { get; set; }

        // List of beds for the dropdown
        public List<SelectListItem>? BedList { get; set; }

        //[Required(ErrorMessage = "Ward selection is required")]
        public string? SelectedWard { get; set; }

        //[Required(ErrorMessage = "Bed selection is required")]
        public string? SelectedBed { get; set; }

        public List<Ward> Ward { get; set; }

      
        public int? SurgeonID { get; set; }
        // Navigation property
        [ForeignKey("SurgeonID")]
        public virtual Surgeon Surgeons { get; set; }

   
        public int? AnaesthesiologistID { get; set; }

        // Navigation property
        [ForeignKey("AnaesthesiologistID")]
        public virtual Anaesthesiologist Anaesthesiologists { get; set; }


        //[BindNever]
        //public IEnumerable<SelectListItem> WardList { get; set; }
        //public IEnumerable<SelectListItem> Surgery_TreatmentCode { get; set; }





    }
}
