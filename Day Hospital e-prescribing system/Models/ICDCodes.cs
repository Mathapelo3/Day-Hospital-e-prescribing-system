using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{

    [Table("ICD_10_Code")]
    public class ICDCodes
    {
       
            [Key]
            public int ICD_ID { get; set; }

            [Required]
            [StringLength(100)]
            public string Description { get; set; }

        public ICollection<Medication_Interaction> Medication_Interaction { get; set; }


    }
}
