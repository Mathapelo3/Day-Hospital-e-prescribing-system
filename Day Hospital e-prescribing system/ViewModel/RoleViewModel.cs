using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class RoleViewModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; }

        public List <Role> Roles { get; set; }
    }
}
