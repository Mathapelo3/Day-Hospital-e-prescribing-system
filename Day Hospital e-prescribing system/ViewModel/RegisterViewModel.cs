using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Xml.Linq;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class RegisterViewModel
    {
        

       
        public string Username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int RoleId { get; set; }

    }
}
