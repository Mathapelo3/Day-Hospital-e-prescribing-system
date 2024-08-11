using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class HospitalRecordViewModel
    {
        // HospitalRecord properties
        public int HospitalRecordID { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }

        public string PM { get; set; }
        public string PMEmail{ get; set; }

        // Suburb properties

        public int SuburbID { get; set; }
        public string SuburbName { get; set; }
        public string PostalCode { get; set; }

        // City properties
        public string CityName { get; set; }

        public List<SelectListItem> Suburbs { get; set; } = new List<SelectListItem>();
    }

}
