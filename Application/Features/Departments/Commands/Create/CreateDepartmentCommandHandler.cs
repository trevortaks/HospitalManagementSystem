using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Departments.Commands.Create;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = _mapper.Map<Department>(request);
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);
        return department.DepartmentId;
    }
}

