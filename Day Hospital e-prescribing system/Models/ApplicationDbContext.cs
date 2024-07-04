using Microsoft.EntityFrameworkCore;

namespace Day_Hospital_e_prescribing_system.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<HospitalRecord> HospitalRecords { get; set; }
        public DbSet<Suburb> Suburbs { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Surgeon> Surgeons { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Anaesthesiologist> Anaesthesiologists { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Theatre> Theatres { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<ICDCodes> ICDCodes { get; set; }
        public DbSet<Active_Ingredient>Active_Ingredient { get; set; }
        public DbSet<Medication_Interaction>Medication_Interaction { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations if needed

            modelBuilder.Entity<Medication_Interaction>()
           .HasKey(m => new { m.ICD_ID, m.Active_IngredientID }); 
        }
    }
}
