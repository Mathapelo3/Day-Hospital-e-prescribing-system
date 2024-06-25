namespace Day_Hospital_e_prescribing_system.Models
{
    public class HospitalRecordViewModel
    {
        public IEnumerable<HospitalRecord> HospitalRecords { get; set; }
        public IEnumerable<Suburb> Suburbs { get; set; }
    }

}
