using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class MedicationType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MedTypeId { get; set; }

        public string DosageForm { get; set; }

        public ICollection<DayHospitalMedication> DayHospitalMedications { get; set; }  
    }
}
