using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Update;

public class UpdateDoctorCommandHandler : Common.Interfaces.IRequestHandler<UpdateDoctorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateDoctorCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId, cancellationToken);
        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        _mapper.Map(request, doctor);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
