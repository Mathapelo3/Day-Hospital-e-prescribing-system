using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Suburb
    {
        [Key]
        public int SuburbID { get; set; }

        [Required]
        [StringLength(100)]
        public string SuburbName { get; set; }

        // Navigation property
        public virtual ICollection<HospitalRecord> HospitalRecords { get; set; }
    }
}
