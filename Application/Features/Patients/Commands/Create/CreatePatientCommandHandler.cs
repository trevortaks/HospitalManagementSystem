using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Exceptions;

namespace HospitalManagementSystem.Application.Features.Patients.Commands.Create;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePatientCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate patient
        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.FirstName == request.FirstName &&
                                      p.LastName == request.LastName &&
                                      p.DateOfBirth == request.DateOfBirth, cancellationToken);

        if (existingPatient != null)
        {
            throw new DuplicateEntityException("Patient", "FullName", $"{request.FirstName} {request.LastName}");
        }

        var patient = _mapper.Map<Patient>(request);
        patient.CreatedAt = DateTime.UtcNow;
        patient.IsActive = true;

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return patient.PatientId;
    }
}