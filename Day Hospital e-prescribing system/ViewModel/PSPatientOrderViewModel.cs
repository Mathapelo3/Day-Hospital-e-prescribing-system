using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PSPatientOrderViewModel
    {
        public int PatientID { get; set; }
        public List<PatientDropDownViewModel> Patients { get; set; }
        public List<PostSurgeryOrderViewModel> Orders { get; set; }
    }
}
