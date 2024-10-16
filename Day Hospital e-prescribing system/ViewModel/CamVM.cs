﻿using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class CamVM
    {
        public Patient? Patient { get; set; }
        public int PatientID { get; set; }  
        public string IDNo { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public List<Active_Ingredient> Active_Ingredient { get; set; }
        public List<General_Medication> General_Medication { get; set; }
        public List<Condition> Conditions { get; set; }
        public List<Vitals> Vitals { get; set; }

        // Constructor to initialize lists
        public CamVM()
        {
            Active_Ingredient = new List<Active_Ingredient>();
            General_Medication = new List<General_Medication>();
            Conditions = new List<Condition>();
            Vitals = new List<Vitals>();
        }

      
    }
}
