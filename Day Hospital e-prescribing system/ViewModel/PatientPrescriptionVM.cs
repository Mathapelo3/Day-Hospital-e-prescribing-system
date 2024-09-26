using Day_Hospital_e_prescribing_system.Models;

namespace Day_Hospital_e_prescribing_system.ViewModel
{
    public class PatientPrescriptionVM
    {
        public int PrescriptionID { get; set; }
        public string Instruction { get; set; }
        public DateTime Date { get; set; }
        public string Quantity { get; set; }

        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }

        public int StockID { get; set; }
        public string DayHospitalMedicationName { get; set; }
        public int MedicationID { get; set; }
        public string MedicationName { get; set; }
        public int QtyLeft { get; set; }

        public int AllergyID { get; set; }
        public string AllergyName { get; set; }
        public string Description { get; set; }

        public int ConditionID { get; set; }
        public string ConditionName { get; set; }

        public int Patient_VitalsID { get; set; }
        public DateTime VitalDate { get; set; }
        public TimeSpan VitalTime { get; set; }
        public decimal MaxVital { get; set; }
        public decimal MinVital { get; set; }
        public string Vital { get; set; }
        public decimal BodyTemp { get; set; }
        public int HeartRate { get; set; }
        public string BloodPressure { get; set; }
        public int Oxygen { get; set; }


        //public DayHospitalMedicationVM DayHospitalMedication { get; set; }

        public Patient? Patient { get; set; }
        public List<Allergy> Allergies { get; set; } = new List<Allergy>();
        public List<Vitals> Vitals { get; set; } = new List<Vitals>();
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        public PatientPrescriptionVM()
        {
            Allergies = new List<Allergy>();
            Vitals = new List<Vitals>();
            Conditions = new List<Condition>();
        }



        public static int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            // If the person hasn't had their birthday this year, subtract 1
            if ((today.Month < dob.Month) ||
                (today.Month == dob.Month && today.Day < dob.Day))
            {
                age--;
            }

            return age;
        }

    }
   
}
