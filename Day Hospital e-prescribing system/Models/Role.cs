using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Role
    {
        [Required]
        public int RoleId { get; set; }

        [Required]

        public string Name { get; set; }
    }
}
