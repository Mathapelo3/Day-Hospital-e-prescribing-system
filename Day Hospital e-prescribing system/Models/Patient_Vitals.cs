using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Patient_Vitals
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Patient_VitalsID { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [StringLength(50)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time is required.")]
        [StringLength(50)]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Notes are required.")]
        [StringLength(200)]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Values are required.")]
        [StringLength(50)]
        public string Value { get; set; }

        [StringLength(50)]
        public string Alert { get; set; }

        [StringLength(50)]
        public string Height { get; set; }

        [StringLength(50)]
        public string Weight { get; set; }

        [Required]
        public int VitalsID { get; set; }
        // Navigation property
        [ForeignKey("VitalsID")]
        public virtual Vitals Vitals { get; set; }

        [Required]
        public int PatientID { get; set; }
        // Navigation property
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }
    }
}
