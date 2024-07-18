using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]

        public string Name { get; set; }
    }
}
