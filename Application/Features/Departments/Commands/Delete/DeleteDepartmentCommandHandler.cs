using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Delete;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == request.DepartmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException("Department", request.DepartmentId);
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

