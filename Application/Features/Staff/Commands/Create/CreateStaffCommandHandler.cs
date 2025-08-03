using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Staff.Commands.Create;

public class CreateStaffCommandHandler : Common.Interfaces.IRequestHandler<CreateStaffCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateStaffCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = _mapper.Map<Domain.Entities.Staff>(request);
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync(cancellationToken);
        return staff.StaffId;
    }
}

