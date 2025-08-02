using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Delete;

public class DeleteAppointmentCommandHandler : IRequestHandler<DeleteAppointmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment", request.AppointmentId);
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

