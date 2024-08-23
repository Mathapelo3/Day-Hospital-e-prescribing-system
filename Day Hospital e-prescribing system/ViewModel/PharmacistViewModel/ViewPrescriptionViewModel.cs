using Day_Hospital_e_prescribing_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Day_Hospital_e_prescribing_system.ViewModel.PharmacistViewModel
{
    public class PrescriptionViewModel
    {
        public class PrescriptionViewModel
        {
            public PrescriptionViewModel()
            {
                PatientList = new List<SelectListItem>();
                MedicationList = new List<SelectListItem>();
            }

            [Required]
            public int PrescriptionID { get; set; }
            public string Medication { get; set; }
            public string Instruction { get; set; }
            public DateTime Date { get; set; }
            public string Quantity { get; set; }
            public string Status { get; set; }
            public bool Urgency { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patient { get; set; }

            public string Surgeon { get; set; }
            [Required]
            public int PatientID { get; set; }
            [Required]
            public int SurgeryID { get; set; }
            [Required]
            public int MedicationID { get; set; }

            public List<PrescriptionViewModel> Prescriptions { get; set; }

            public int SelectedPatientId { get; set; }
            public IEnumerable<SelectListItem> PatientList { get; set; }
            public IEnumerable<SelectListItem> MedicationList { get; set; }
            public List<MedicationViewModel> SelectedMedications { get; set; } = new List<MedicationViewModel>();
      

          

            [Required]
            public string Time { get; set; }

            [Required]
            [StringLength(200)]
            public string Notes { get; set; }

            [StringLength(50)]
            public string Height { get; set; }

            [StringLength(50)]
            public string Weight { get; set; }

            




            public List<VitalsViewModel> Vitals { get; set; }
       

       
            public int VitalsID { get; set; }
           
            public string Min { get; set; }
            public string Max { get; set; }
            public string Value { get; set; }
       

       
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Patient_AllergyID { get; set; }

            [Required]
            [ForeignKey("AllergyID")]
            public int AllergyID { get; set; }


           

       
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Patient_ConditionID { get; set; }

            [Required]
            public string ConditionName { get; set; }


            

            [Required]
            public int ConditionID { get; set; }

       

     
       
         

           
        }
    }
}
