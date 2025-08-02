using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Common.Exceptions;

namespace HospitalManagementSystem.Application.Features.Patients.Queries.GetById;

public class GetPatientByIdQueryHandler : Common.Interfaces.IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPatientByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _context.Patients
            .Include(p => p.MedicalRecords)
            .Include(p => p.Appointments)
            .Include(p => p.Bills)
            .FirstOrDefaultAsync(p => p.PatientId == request.Id && p.IsActive, cancellationToken);

        if (patient == null)
        {
            throw new NotFoundException("Patient", request.Id);
        }

        return _mapper.Map<PatientDto>(patient);
    }
}