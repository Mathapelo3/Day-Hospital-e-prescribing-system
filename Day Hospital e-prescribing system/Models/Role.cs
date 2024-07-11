<<<<<<< HEAD
﻿namespace Day_Hospital_e_prescribing_system.Models
{
    public class Role
    {
        public int RoleId { get; set; }
=======
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class Role
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
>>>>>>> changed dayhospitalrecord view page
        public string Name { get; set; }
    }
}
