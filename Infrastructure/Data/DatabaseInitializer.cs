using HospitalManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using HospitalManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HospitalManagementSystem.Infrastructure.Data;

/// <summary>
/// Seeds initial data for the application, including a default super administrator user.
/// </summary>
public class DatabaseInitializer
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public DatabaseInitializer(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Ensures that essential roles and the default super administrator user exist.
    /// </summary>
    public async Task InitializeAsync()
    {
        const string adminRole = "Administrator";
        const string adminUser = "superadmin";
        const string adminEmail = "superadmin@example.com";
        const string adminPassword = "SuperAdmin123!";

        if (!await _roleManager.RoleExistsAsync(adminRole))
        {
            var role = new ApplicationRole { Name = adminRole, Description = "System administrator" };
            await _roleManager.CreateAsync(role);
        }

        var user = await _userManager.FindByNameAsync(adminUser);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminUser,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(user, adminPassword);
            await _userManager.AddToRoleAsync(user, adminRole);
        }
    }

    /// <summary>
    /// Seeds sample data for development environment.
    /// </summary>
    public async Task SeedDevelopmentDataAsync()
    {
        if (!_context.Departments.Any())
        {
            var cardiology = new Department
            {
                Name = "Cardiology",
                Description = "Heart related services",
                Location = "Building A"
            };

            var neurology = new Department
            {
                Name = "Neurology",
                Description = "Brain and nerves",
                Location = "Building B"
            };

            _context.Departments.AddRange(cardiology, neurology);
            await _context.SaveChangesAsync();
        }

        if (!_context.Doctors.Any())
        {
            var department = _context.Departments.First();

            var doctor = new Doctor
            {
                FirstName = "John",
                LastName = "Doe",
                Specialization = "Cardiology",
                LicenseNumber = "DOC123456",
                ContactNumber = "555-0000",
                Email = "john.doe@example.com",
                DepartmentId = department.DepartmentId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
        }

        if (!_context.Patients.Any())
        {
            var patient = new Patient
            {
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = new DateTime(1990, 5, 20),
                Gender = "F",
                ContactNumber = "555-1111",
                Address = "123 Main St",
                Email = "jane.smith@example.com"
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
        }

        if (!_context.Appointments.Any())
        {
            var doctor = _context.Doctors.First();
            var patient = _context.Patients.First();

            var appointmentDate = DateTime.UtcNow.AddDays(1);

            var appointment = new Appointment
            {
                DoctorId = doctor.DoctorId,
                PatientId = patient.PatientId,
                AppointmentDate = appointmentDate,
                EndTime = appointmentDate.AddHours(1),
                Purpose = "General Checkup",
                Status = "Scheduled"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
