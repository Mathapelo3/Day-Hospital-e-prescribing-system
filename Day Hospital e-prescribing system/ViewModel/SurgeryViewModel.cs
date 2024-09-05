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
            SurgeonList = new List<SelectListItem>();
            TheatreList = new List<SelectListItem>();
            TreatmentCodeList = new List<SelectListItem>();
            PatientList = new List<SelectListItem>();
            SelectedTreatmentCodes = new List<int>();
        }

        //public int SurgeryID { get; set; }

        public DateTime Date { get; set; }

        public string Time { get; set; }
        public int PatientID { get; set; }
        public int TreatmentCodeID { get; set; }

        public int TheatreID { get; set; }

        public int AnaesthesiologistID { get; set; }
        public int SurgeonID { get; set; }
        public IEnumerable<SelectListItem> SurgeonList { get; set; }

        public IEnumerable<SelectListItem> AnaesthesiologistList { get; set; }
        public IEnumerable<SelectListItem> TreatmentCodeList { get; set; }
        public IEnumerable<SelectListItem> TheatreList { get; set; }
        public IEnumerable<SelectListItem> PatientList { get; set; }
        public List<int> SelectedTreatmentCodes { get; set; }
    }
}
