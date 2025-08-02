using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Update;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == request.RoomId, cancellationToken);
        if (room == null)
        {
            throw new NotFoundException("Room", request.RoomId);
        }

        _mapper.Map(request, room);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

