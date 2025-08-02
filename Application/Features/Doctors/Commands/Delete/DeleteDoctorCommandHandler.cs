using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Doctors.Commands.Delete;

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteDoctorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId, cancellationToken);
        if (doctor == null)
        {
            throw new NotFoundException("Doctor", request.DoctorId);
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
