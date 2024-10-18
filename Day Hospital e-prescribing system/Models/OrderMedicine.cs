using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    [Table("OrderMedicine")]
    public class OrderMedicine
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime Date { get; set; }

        public bool Urgency { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; }

        public int StockID { get; set; } // This should match the database schema

        [ForeignKey("StockID")]
        public DayHospitalMedication DayHospitalMedication { get; set; }
    }

}
