using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("City")]
    public class City
    {

        [Key]
        public int CityID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int ProvinceID { get; set; }

        // Navigation property
        [ForeignKey("ProvinceID")]
        public virtual Province Province { get; set; }
    }
}
