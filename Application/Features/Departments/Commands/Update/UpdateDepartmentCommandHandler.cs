using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Update;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == request.DepartmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException("Department", request.DepartmentId);
        }

        _mapper.Map(request, department);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

