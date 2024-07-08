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
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Nurse> Nurses { get; set; }
        public DbSet<Surgeon> Surgeons { get; set; }
        public DbSet<Pharmacist> Pharmacists { get; set; }
        public DbSet<Anaesthesiologist> Anaesthesiologists { get; set; }
        public DbSet<Specialization> Specializations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configurations if needed
        }
    }
}
