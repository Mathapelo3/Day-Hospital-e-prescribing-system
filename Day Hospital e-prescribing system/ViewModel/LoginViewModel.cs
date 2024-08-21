using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class LoginViewModel
    {
        //public string Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email{ get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
