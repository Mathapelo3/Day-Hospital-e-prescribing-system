using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using WebApplication27.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdmittedPatientsVM
    {

        public int AdmissionID { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }

        // Patient Information
        public int PatientID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }

        // Bed Information
        public string BedName { get; set; }  // Assuming Bed has a name or number you want to display
        public int? BedId { get; set; }  // BedId if you need to track the selected Bed
        public string Bed { get; set; }  // List of all available beds

        // Treatment Information
        public string TreatmentCodes { get; set; }  // Assuming this is a string or description of the treatment
        public int TreatmentCodeID { get; set; }

        // Anaesthesiologist and Surgeon Information
        public string Anaesthesiologists { get; set; }
        public string Surgeons { get; set; }
        public string Patients { get; set; }

    }
}
