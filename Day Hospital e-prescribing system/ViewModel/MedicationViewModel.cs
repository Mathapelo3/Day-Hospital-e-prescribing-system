using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class MedicationViewModel
    {
        public int MedicationID { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string Instruction { get; set; } // For new prescription

    }
}
