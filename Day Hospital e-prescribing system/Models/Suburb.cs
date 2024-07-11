using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Suburb")]
    public class Suburb
    {
        [Key]
        public int SuburbID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(50)]
        public string PostalCode { get; set; }

        [Required]
        public int CityID { get; set; }

        // Navigation property
        [ForeignKey("CityID")]
        public virtual City City { get; set; }
 
    }
}
