using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Discharge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DischargeID { get; set; }

        [Required]
        [StringLength(50)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Time { get; set; }

        [Required]
        public int AdmissionID { get; set; }

        // Navigation property
        [ForeignKey("AdmissionID")]
        public virtual Admission Addmissions { get; set; }
    }
}
