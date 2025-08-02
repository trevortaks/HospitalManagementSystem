using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Doctors.Queries.GetById;

public class GetDoctorByIdQueryHandler : Common.Interfaces.IRequestHandler<GetDoctorByIdQuery, DoctorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDoctorByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DoctorDto> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId, cancellationToken);

        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        return _mapper.Map<DoctorDto>(doctor);
    }
}
