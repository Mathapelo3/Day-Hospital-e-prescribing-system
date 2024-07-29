using Microsoft.EntityFrameworkCore;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<HospitalRecord> HospitalRecords { get; set; }
        public DbSet<Suburb> Suburbs { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
        

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Surgeon> Surgeons { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Anaesthesiologist> Anaesthesiologists { get; set; }
        public DbSet<Vitals> Vitals { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Theatre> Theatres { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<ICDCodes> ICDCodes { get; set; }
        public DbSet<Active_Ingredient>Active_Ingredient { get; set; }
        public DbSet<Medication_Interaction>Medication_Interaction { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Surgery> Surgeries { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<General_Medication> General_Medication { get; set; }
        public DbSet<Surgery_TreatmentCode> Surgery_TreatmentCodes { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<TreatmentCode> TreatmentCodes { get; set; }
        public DbSet<Discharge> Discharges { get; set; }
        public DbSet<Patient_Vitals> Patient_Vitals { get; set; }

        public DbSet<Patient_Allergy> Patient_Allergy { get; set; }

        public DbSet<Patient_Condition> Patient_Condition { get; set; }

        public DbSet<Patient_Medication> Patient_Medication { get; set; }

        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations if needed
            modelBuilder.Entity<User>()
                        .ToTable("User");

            modelBuilder.Entity<Role>()
                        .ToTable("Role");

            modelBuilder.Entity<Patient>()
                        .ToTable("Patient");

            modelBuilder.Entity<Patient_Condition>()
           .ToTable("Patient_Condition");

            modelBuilder.Entity<Medication_Interaction>()
           .HasKey(m => new { m.ICD_ID, m.Active_IngredientID }); 
        }
    }
}
