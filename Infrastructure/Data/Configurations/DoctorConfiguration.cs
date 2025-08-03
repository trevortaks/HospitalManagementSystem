using System.Reflection.Emit;
using HospitalManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagementSystem.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.DoctorId);
        builder.HasQueryFilter(d => d.Status == "Active");
        // Doctor → Department (many-to-one)
        builder
            .HasOne(d => d.Department)
            .WithMany(dep => dep.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        // Doctor → HeadDoctor (self-referencing many-to-one)
        builder
            .HasOne(d => d.HeadDoctor)
            .WithMany(h => h.SubordinateDoctors)
            .HasForeignKey(d => d.HeadDoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
