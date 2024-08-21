using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Province")]
    public class Province
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProvinceID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
