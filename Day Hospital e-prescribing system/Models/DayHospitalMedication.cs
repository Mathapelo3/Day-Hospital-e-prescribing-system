using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class DayHospitalMedication
    {
        [Key]
        public int StockID {get;set;}
        
        public int QtyReceived { get;set;}

<<<<<<< HEAD
        //public int QtyUsed { get; set; }
=======
        
>>>>>>> 00d482360770eca0b7920ad78edc9ea630c1e452

        public int QtyLeft { get;set;}

        public int ReOrderLevel { get;set;}

        public string MedicationName { get;set;}

        public int Schedule {  get;set;}

<<<<<<< HEAD
        //public string DosageForm { get;set;}
=======
        
>>>>>>> 00d482360770eca0b7920ad78edc9ea630c1e452

        public int MedTypeId { get; set; }
        [ForeignKey("MedTypeId")]
        public MedicationType MedicationTypes { get; set; }

        [ForeignKey("Schedule")]
        public Medication_Schedule MedicationSchedule { get; set; }


    }
}
