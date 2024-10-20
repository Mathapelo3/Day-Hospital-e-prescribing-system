using Day_Hospital_e_prescribing_system.Models;
using static Day_Hospital_e_prescribing_system.ViewModel.CamVM;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class CamVM
    {
        public Patient? Patient { get; set; }
        public int PatientID { get; set; }  
        public string Name { get; set; }
        public string Surname { get; set; }
        public List<ActiveIngredientViewModel> Active_Ingredients { get; set; }
        public List<ConditionViewModel> Conditions { get; set; }
        public List<MedicationViewModel> General_Medications { get; set; }

        // Constructor to initialize lists
        public CamVM()
        {
            Active_Ingredients = new List<ActiveIngredientViewModel>();
            Conditions = new List<ConditionViewModel>();
            General_Medications = new List<MedicationViewModel>();
        }

        public class ActiveIngredientViewModel
        {
            public string Active_IngredientDescription { get; set; }
        }

        public class ConditionViewModel
        {
            public string ConditionName { get; set; }
        }

        public class MedicationViewModel
        {
            public string MedicationName { get; set; }
        }

    }
}
