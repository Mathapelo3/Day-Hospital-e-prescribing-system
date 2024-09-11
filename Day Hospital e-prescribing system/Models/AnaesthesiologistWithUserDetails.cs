namespace Day_Hospital_e_prescribing_system.Models
{
    public class AnaesthesiologistWithUserDetails
    {
        public int AnaesthesiologistID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }
    }
}
