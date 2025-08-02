using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Staff.Queries.GetById;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, StaffDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStaffByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StaffDto> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff.AsNoTracking()
            .FirstOrDefaultAsync(s => s.StaffId == request.StaffId, cancellationToken);

        if (staff == null)
        {
            throw new NotFoundException("Staff", request.StaffId);
        }

        return _mapper.Map<StaffDto>(staff);
    }
}

