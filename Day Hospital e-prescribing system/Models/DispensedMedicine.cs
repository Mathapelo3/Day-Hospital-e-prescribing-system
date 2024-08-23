using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class DispensedMedicine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Dispensed_MedicineID { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        [StringLength(50)]
        public string Quantity { get; set; }

        [Required]
        public int PrescriptionID { get; set; }
        // Navigation property
        [ForeignKey("PrescriptionID")]
        public virtual Prescription Prescription { get; set; }

        [Required]
        public int PharmacistID { get; set; }
        // Navigation property
        [ForeignKey("PharmacistID")]
        public virtual Pharmacist Pharmacist { get; set; }


        [Required]
        public int DayHospitalMedId { get; set; }
        // Navigation property
        [ForeignKey("StockID")]
        public virtual DayHospitalMedication DayHospitalMedicine { get; set; }

    }
}
