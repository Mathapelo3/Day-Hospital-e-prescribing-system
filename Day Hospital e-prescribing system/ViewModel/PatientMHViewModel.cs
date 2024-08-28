using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientMHViewModel
    {

        public int PatientID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public List<VitalViewModel> Vitals { get; set; }
        public List<PAllergyViewModel> Allergies { get; set; }
        public List<PConditionViewModel> Conditions { get; set; }
        public List<PMedicationViewModel> Medications { get; set; }
    }

    public class VitalViewModel
    {
        public DateTime VitalDate { get; set; }
        public TimeSpan VitalTime { get; set; }
        public string VitalHeight { get; set; }
        public string VitalWeight { get; set; }
        public string VitalName { get; set; }
        public string VitalValue { get; set; }
        public string VitalNotes { get; set; }
    }

    public class PAllergyViewModel
    {
        public string AllergyName { get; set; }
    }

    public class PConditionViewModel
    {
        public string ConditionName { get; set; }
    }

    public class PMedicationViewModel
    {
        public string MedicationName { get; set; }
    }
}

