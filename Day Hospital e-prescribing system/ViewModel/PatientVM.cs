using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientVM
    {
        public int AdmissionID { get; set; }
        public int PatientID { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? Surname { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "ID Number is required")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be 13 digits")]
        public string? IDNo { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Address Line 1 is required")]
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$")]
        public string? ContactNo { get; set; }

        [Required(ErrorMessage = "Next of Kin Contact Number is required")]
        public string? NextOfKinNo { get; set; }
        public string? Status { get; set; }
        public int? TreatmentCodeID { get; set; }
        public int SuburbID { get; set; }
        public int? BedId { get; set; }

        public string? SuburbName { get; set; }
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "City is required")]
        public int CityID { get; set; }
        public string? CityName { get; set; }
        public IEnumerable<SelectListItem>? Cities { get; set; }

        [Required(ErrorMessage = "Province is required")]
        public int ProvinceID { get; set; }
        public string? ProvinceName { get; set; }
        public IEnumerable<SelectListItem>? Provinces { get; set; }
       

        public List<SelectListItem>? Suburbs { get; set; } = new List<SelectListItem>();

        //public IEnumerable<City> Cities { get; set; }
        public IEnumerable<SelectListItem>? SuburbList { get; set; }
       
    }
}
