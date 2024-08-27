using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientVM
    {
        public int AdmissionID { get; set; }
        public int PatientID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string IDNo { get; set; }
        public string Gender { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string NextOfKinNo { get; set; }
        public string Status { get; set; }
        public int? TreatmentCodeID { get; set; }
        public int SuburbID { get; set; }
        public int? BedId { get; set; }

        public string SuburbName { get; set; }
        public string PostalCode { get; set; }

        // City properties
        public int CityID { get; set; }
        public string CityName { get; set; }

        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }

        public List<SelectListItem> Suburbs { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> CityList { get; set; }
        public IEnumerable<SelectListItem> SuburbList { get; set; }
        public IEnumerable<SelectListItem> ProvinceList { get; set; }
    }
}
