using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdministerMedicationVM
    {
        public int PatientID { get; set; }
        public int PrescriptionID { get; set; }
        public int NurseID { get; set; }
        public string? Medication { get; set; }
        public string? Quantity { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Administer status is required.")]
        public string Administer { get; set; }



    }
}
