using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;

namespace HospitalManagementSystem.Application.Features.Patients.Commands.Update;

public class UpdatePatientCommandHandler : Common.Interfaces.IRequestHandler<UpdatePatientCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePatientCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == request.PatientId && p.IsActive, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.PatientId);
        }

        _mapper.Map(request, patient);
        patient.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}