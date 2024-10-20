using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("TreatmentCode")]
    public class TreatmentCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TreatmentCodeID { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }
        [Required]
        [StringLength(50)]
        public string ICD_10_Code { get; set; }

        //[Required]
        //public int Surgery_TreatmentCodeID { get; set; }
        //// Navigation property
        //[ForeignKey("Surgery_TreatmentCode")]
        //public virtual Surgery_TreatmentCode Surgery_TreatmentCodes { get; set; }
    }
}
