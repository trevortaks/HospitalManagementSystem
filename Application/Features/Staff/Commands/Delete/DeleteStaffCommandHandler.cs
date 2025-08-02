using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Delete;

public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == request.StaffId, cancellationToken);
        if (staff == null)
        {
            throw new NotFoundException("Staff", request.StaffId);
        }

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

