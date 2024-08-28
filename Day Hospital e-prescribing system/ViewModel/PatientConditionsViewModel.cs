using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientConditionsViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_ConditionID { get; set; }

        [Required]
        public string ConditionName { get; set; }


        [Required]
        public int PatientID { get; set; }
        

        [Required]
        public int ConditionID { get; set; }
       
    }
}
