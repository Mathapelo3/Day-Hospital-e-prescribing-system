﻿namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class BookedSurgeriesVM
    {
        public int SurgeryID { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int PatientID { get; set; }
        public string? PatientName { get; set; }
        public string? PatientSurname { get; set; }
        public int AnaesthesiologistID { get; set; }
        public int UserID { get; set; }
        public string? AnaesthesiologistName { get; set; }
        public string? AnaesthesiologistSurname { get; set; }
        public string? SurgeonName { get; set; }
        public string SurgeonSurname { get; set; }
        public int TheatreID { get; set; }
        public string? TheatreName { get; set; }
        public string? ICD_10_Code { get; set; }
        public string? Description { get; set; }
        public bool Administered { get; set; } // Added this property
    }
}