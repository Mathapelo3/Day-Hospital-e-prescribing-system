<<<<<<< HEAD
﻿using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required]
        [StringLength(50)]
        public string DateOfBirth { get; set; }

        [Required]
        [StringLength(50)]
        public string IDNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Gender { get; set; }

        [Required]
        [StringLength(50)]
        public string AddressLine1 { get; set; }

        [Required]
        [StringLength(50)]
        public string AddressLine2 { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(20)]
        public string NextOfKinNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        public int WardID { get; set; }
        // Navigation property
        [ForeignKey("WardID")]
        public virtual Ward Wards { get; set; }

        [Required]
        public int TreatmentCode { get; set; }

        [Required]
        public int SuburbID { get; set; }
        // Navigation property
        [ForeignKey("SuburbID")] 
        public virtual Suburb Suburbs { get; set; }
=======
﻿namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }
        public string Patient { get; set; }
        
        public DateTime Date { get; set; }
        public string Time { get; set; }
  
        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Nurse { get; set; }
        public string Status { get; set; }
>>>>>>> 6a7aedb61832e4eb858ec8c91d6c35faf1f2cb33
    }
}
