﻿using Day_Hospital_e_prescribing_system.ViewModels;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientPrescriptionWithRelatedDataVM
    {
        public RejectedPrescriptionVM RejectedPrescription { get; set; }
        public IEnumerable<PatientPrescriptionVM> Prescription { get; set; }
        public IEnumerable<PatientAllergiesViewModel> Allergies { get; set; }
        public IEnumerable<PatientConditionsViewModel> Conditions { get; set; }
        public IEnumerable<PatientVitalsViewModel> Vitals { get; set; }
        public IEnumerable<PatientMedicationVM> Medications { get; set; }

        public string AllergyAlert { get; set; }
        public string ContraIndicationAlert { get; set; }
    }
}
