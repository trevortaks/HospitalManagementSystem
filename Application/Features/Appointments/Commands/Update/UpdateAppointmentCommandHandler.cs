using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Application.Features.Appointments.Commands.Update;

public class UpdateAppointmentCommandHandler : Common.Interfaces.IRequestHandler<UpdateAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentId == request.AppointmentId, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment", request.AppointmentId);
        }

        _mapper.Map(request, appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

