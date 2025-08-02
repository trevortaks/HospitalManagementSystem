using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Staff.Queries.GetAll;

public class GetAllStaffQueryHandler : IRequestHandler<GetAllStaffQuery, List<StaffDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllStaffQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<StaffDto>> Handle(GetAllStaffQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<StaffDto>>(staff);
    }
}

