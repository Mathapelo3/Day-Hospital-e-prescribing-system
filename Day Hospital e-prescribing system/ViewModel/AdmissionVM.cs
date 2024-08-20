using Day_Hospital_e_prescribing_system.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class AdmissionVM
    {
        public int AdmissionID { get; set; }

      
        public int TreatmentCodeID { get; set; }


        [DataType(DataType.Date)]
        //[StringLength(50)]
        public DateTime Date { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan Time { get; set; }

        public int PatientID { get; set; }

        //public string Ward { get; set; }    

        public string BedName { get; set; }

        public string WardName { get; set; }

        public string Description { get; set; }

        public int WardId { get; set; }
        public int BedId { get; set; }
        public virtual Ward Ward { get; set; }

        //public int NumberOfBeds { get; set; }
        public bool IsAvailable { get; set; }

        public virtual ICollection<Bed> Bed { get; set; }


        public IEnumerable<SelectListItem> TreatmentCode { get; set; }





    }
}
