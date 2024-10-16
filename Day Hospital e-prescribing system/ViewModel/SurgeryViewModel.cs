using Day_Hospital_e_prescribing_system.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class SurgeryViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; }

        public int PatientID { get; set; }

        public int SurgeonID { get; set; }

        public int AnaesthesiologistID { get; set; }

        public int TheatreID { get; set; }

        public List<string> SelectedTreatmentCodes { get; set; }

        public List<SelectListItem> PatientList { get; set; }
        public List<SelectListItem> SurgeonList { get; set; }
        public List<SelectListItem> AnaesthesiologistList { get; set; }
        public List<SelectListItem> TheatreList { get; set; }
        public List<SelectListItem> TreatmentCodeList { get; set; }
    }
}
