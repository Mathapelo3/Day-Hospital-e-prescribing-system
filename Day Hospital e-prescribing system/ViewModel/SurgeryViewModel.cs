using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class SurgeryViewModel
    {
        public SurgeryViewModel()
        {
            AnaesthesiologistList = new List<SelectListItem>();
            TheatreList = new List<SelectListItem>();
            TreatmentCodeList = new List<SelectListItem>();
            PatientList = new List<SelectListItem>();
            SelectedTreatmentCodes = new List<int>(); 
        }

        //public int SurgeryID { get; set; }

        public DateTime Date { get; set; }

        public string Time { get; set; }

        //[StringLength(100)]
        //public string Description { get; set; }

        //public bool Urgency { get; set; }

        //public bool Administered { get; set; }

        //[StringLength(200)]
        //public string QAdministered { get; set; }
        public int PatientID { get; set; }
        public int SurgeonID { get; set; }
        public int SurgeryID { get; set; }

        public int TheatreID { get; set; }

        public int AnaesthesiologistID { get; set; }


        //public int WardID { get; set; }
        //// Navigation property
        //[ForeignKey("WardID")]
        //public virtual Ward Wards { get; set; }

        //public int SurgeonID { get; set; }
        //// Navigation property
        //[ForeignKey("SurgeonID")]
        //public virtual Surgeon Surgeons { get; set; }

        //public int NurseID { get; set; }
        //// Navigation property
        //[ForeignKey("NurseID")]
        //public virtual Nurse Nurses { get; set; }

        public IEnumerable<SelectListItem> AnaesthesiologistList { get; set; }
        public IEnumerable<SelectListItem> TreatmentCodeList { get; set; }
        public IEnumerable<SelectListItem> TheatreList { get; set; }
        public IEnumerable<SelectListItem> PatientList { get; set; }

        // New collection for selected treatment codes
        public List<int> SelectedTreatmentCodes { get; set; }
    }
}
