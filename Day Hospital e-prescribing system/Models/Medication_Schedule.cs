using System.ComponentModel.DataAnnotations;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Medication_Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        public int schedule {  get; set; }
    }
}
