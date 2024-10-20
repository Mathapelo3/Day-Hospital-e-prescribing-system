using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Day_Hospital_e_prescribing_system.ViewModel;

namespace Day_Hospital_e_prescribing_system.ViewModels
{
    public class PatientVitalsViewModel
    {
        [Key]
        [Required]
        public int PrescriptionID { get; set; }
        public DateTime Date { get; set; }

        [Required]
        public string Time { get; set; }

        [Required]
        [StringLength(200)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string Height { get; set; }

        [StringLength(50)]
        public string Weight { get; set; }

        [Required]
        public int PatientID { get; set; }

       
        public decimal BodyTemp { get; set; }

        public TimeSpan VitalTime { get; set; }
        public int HeartRate { get; set; }

        public string BloodPressure { get; set; }

        public int Oxygen {  get; set; }






        public List<VitalsViewModel> Vitals { get; set; }
    }

    public class VitalViewModel
    {
        public int VitalsID { get; set; }
        public string Name { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Value { get; set; }
    }
}
