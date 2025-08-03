using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Infrastructure.Identity;

namespace HospitalManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Billing> Bills { get; set; }
        public DbSet<BillingItem> BillingItems { get; }
        public DbSet<Payment> Payments { get; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; }
        public DbSet<RoomAssignment> RoomAssignments { get; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<StaffSchedule> StaffSchedules { get; }
        public DbSet<StaffLeave> StaffLeaves { get; }
        public DbSet<StaffPerformance> StaffPerformances { get; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Add timestamps
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                var userName = _currentUserService.UserName ?? "System";

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userName;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userName;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public void MarkAsModified<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void MarkAsDeleted<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}
