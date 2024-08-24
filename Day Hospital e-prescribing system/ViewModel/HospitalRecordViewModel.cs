using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class HospitalRecordViewModel
    {
        // HospitalRecord properties
        public int HospitalRecordID { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNo { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Purchase manager is required")]
        public string PM { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string PMEmail { get; set; }

        // Suburb properties
        [Required(ErrorMessage = "Suburb is required")]
        public int SuburbID { get; set; }
        public string SuburbName { get; set; }
        public string PostalCode { get; set; }

        // City properties
        public string CityName { get; set; }

        public string ProvinceName { get; set; }

        public List<SelectListItem> Suburbs { get; set; } = new List<SelectListItem>();
    }
}
