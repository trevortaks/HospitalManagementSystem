using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Doctors.Queries.GetAll;

public class GetAllDoctorsQueryHandler : Common.Interfaces.IRequestHandler<GetAllDoctorsQuery, List<DoctorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllDoctorsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<DoctorDto>> Handle(GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _context.Doctors.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<DoctorDto>>(doctors);
    }
}
