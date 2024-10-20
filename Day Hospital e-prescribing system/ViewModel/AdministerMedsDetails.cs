﻿namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdministeredMedDetails
    {
        public string MedicationName { get; set; }
        public string Quantity { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string PatientName { get; set; }
        public string Surname { get; set; }
        public string Administer { get; set; }

        public string Instruction { get; set; }

        //public int PatientID { get; set; }
        //public int MedicationID { get; set; }
        //public int PrescriptionID { get; set; }
    }
}