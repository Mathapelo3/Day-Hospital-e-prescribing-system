using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AddPatientsViewModel
    {
        //public int PatientID { get; set; }

        [Required(ErrorMessage = "Please enter patient name.")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter patient surname.")]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter patient date of birth.")]
        [StringLength(50)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Please enter patient ID Number.")]
        [StringLength(50)]
        public string IDNo { get; set; }

        [Required(ErrorMessage = "Please select patient gender.")]
        [StringLength(50)]
        public string Gender { get; set; }

        public string Status { get; set; } = "Booked";
    }
}
