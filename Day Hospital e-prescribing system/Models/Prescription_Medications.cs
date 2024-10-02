namespace Day_Hospital_e_prescribing_system.Models
{
    public class Prescription_Medications
    {
        public int PrescriptionID { get; set; }
        public int StockID { get; set; }
        public int Quantity { get; set; }
        public string InstructionText { get; set; }

        public virtual Prescription Prescription { get; set; }
        public virtual DayHospitalMedication DayHospitalMedication { get; set; }
    }
}
