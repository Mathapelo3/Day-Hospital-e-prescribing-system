using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Day_Hospital_e_prescribing_system.Models
{
    public class General_Medication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int General_MedicationID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Description")]
        public string Description { get; set; }

        [Required]
        public int AdminID { get; set; }

        // Navigation property
        [ForeignKey("AdminID")]
        public virtual Admin Admin { get; set; }

        [Required]
        public int NurseID { get; set; }

        // Navigation property
        [ForeignKey("NurseID")]
        public virtual Nurse Nurse { get; set; }
    }
}
