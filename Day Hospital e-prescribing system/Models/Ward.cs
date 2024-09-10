using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication27.Models;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Ward")] // Specify the table name explicitly if it differs from the default
    public class Ward
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WardId { get; set; }

        [Required]
        public string WardName { get; set; }

        [Required]
        public string NumberOfBeds { get; set; }

        public virtual ICollection<Bed> Bed { get; set; }
    }
}
