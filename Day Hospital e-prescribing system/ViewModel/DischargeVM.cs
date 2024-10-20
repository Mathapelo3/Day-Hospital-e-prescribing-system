using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class DischargeVM
    {
        public int PatientID { get; set; }
        public string? Status { get; set; }
       
        public string? Name { get; set; }
        public string? Surname { get; set; }

       
        public string? IDNo { get; set; }
        public string? Email { get; set; }
    }
}
