using Microsoft.AspNetCore.Mvc.Rendering;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class InsertCamVM
    {
        public string IDNo { get; set; }
        public int PatientID { get; set; } // Assuming PatientID is passed to the ViewModel
        public string Name { get; set; }
        public string Surname { get; set; }

        public string SelectedCondition { get; set; }
        public List<SelectListItem> Conditions { get; set; }

       
        public string SelectedAllergy { get; set; }
        public List<SelectListItem> Allergies { get; set; }


        public string SelectedMedication { get; set; }
        public List<SelectListItem> Medications { get; set; }
    }
}
