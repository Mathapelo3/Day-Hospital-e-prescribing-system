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

        public string? Description { get; set; }

        public string? ICD_10_Code { get; set; }

        //public int? Surgery_TreatmentCodeID { get; set; }
        //// Navigation property
        //[ForeignKey("Surgery_TreatmentCode")]
        //public virtual Surgery_TreatmentCode Surgery_TreatmentCodes { get; set; }

        public IList<Surgery_TreatmentCode>? Surgery_TreatmentCodes { get; set; }

    }
}
