using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Update;

public class UpdateStaffCommandHandler : Common.Interfaces.IRequestHandler<UpdateStaffCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateStaffCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == request.StaffId, cancellationToken);
        if (staff == null)
        {
            throw new NotFoundException("Staff", request.StaffId);
        }

        _mapper.Map(request, staff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

