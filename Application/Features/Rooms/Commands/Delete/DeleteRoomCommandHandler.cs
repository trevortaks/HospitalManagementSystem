using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Delete;

public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoomCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == request.RoomId, cancellationToken);
        if (room == null)
        {
            throw new NotFoundException("Room", request.RoomId);
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

