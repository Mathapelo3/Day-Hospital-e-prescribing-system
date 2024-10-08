﻿using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientRecordViewModel
    {
        public string IDNo { get; set; }
        public Patient? Patient { get; set; }
        public List<Allergy> Allergies { get; set; } = new List<Allergy>();
        public List<Vitals> Vitals { get; set; } = new List<Vitals>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        public PatientRecordViewModel()
        {
            Allergies = new List<Allergy>();
            Vitals = new List<Vitals>();
            Conditions = new List<Condition>();
        }
    }

}
