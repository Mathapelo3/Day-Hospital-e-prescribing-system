using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientMedicationVM
    {
        [Key]
        public int Patient_MedicationID { get; set; }

      
        public int PatientID { get; set; }
        
        public int MedicationID { get; set; }
        public string MedicationName { get; set; }
       
    }
}
