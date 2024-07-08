using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("Active_Ingredient")]
    public class Active_Ingredient
    {

        [Key]
        public int Active_IngredientID { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }
    }
}
