namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class ICD_CodesVM
    {
        public int ICD_ID { get; set; }
        public string Description { get; set; }
        public List<string> Active_Ingredient { get; set; } = new List<string>();
    }
}
