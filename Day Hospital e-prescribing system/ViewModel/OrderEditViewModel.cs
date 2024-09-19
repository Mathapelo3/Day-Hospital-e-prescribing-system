using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class OrderEditViewModel
    {
        [Required]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        public string Quantity { get; set; }

        public string Status { get; set; }

       
        public string PatientName { get; set; }
        public string PatientSurname { get; set; }
        public string MedicationName { get; set; }

    }

}
