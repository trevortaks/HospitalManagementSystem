using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Domain.Entities;
using System.Collections.Generic;

namespace HospitalManagementSystem.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Medication> Medications { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<Billing> Bills { get; }
    DbSet<BillingItem> BillingItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<InsuranceClaim> InsuranceClaims { get; }
    DbSet<Department> Departments { get; }
    DbSet<Room> Rooms { get; }
    DbSet<RoomType> RoomTypes { get; }
    DbSet<RoomAssignment> RoomAssignments { get; }
    DbSet<Staff> Staff { get; }
    DbSet<StaffSchedule> StaffSchedules { get; }
    DbSet<StaffLeave> StaffLeaves { get; }
    DbSet<StaffPerformance> StaffPerformances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    void MarkAsModified<TEntity>(TEntity entity) where TEntity : class;

    void MarkAsDeleted<TEntity>(TEntity entity) where TEntity : class;
}