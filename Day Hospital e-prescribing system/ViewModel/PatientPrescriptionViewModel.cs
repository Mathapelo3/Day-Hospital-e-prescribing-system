﻿namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientPrescriptionViewModel
    {
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public List<PrescriptionDetails> Prescriptions { get; set; }

        public class PrescriptionDetails
        {
            public int PrescriptionID { get; set; }
            public string InstructionText { get; set; }
            public DateTime Date { get; set; }
            public int? Quantity { get; set; }
            public string Status { get; set; }
            public bool Urgency { get; set; }
            public string MedicationName { get; set; }
            
             
        }
    }

}
