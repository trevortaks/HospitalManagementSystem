using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Rooms.Commands.Create;

public class CreateRoomCommandHandler : Common.Interfaces.IRequestHandler<CreateRoomCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateRoomCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = _mapper.Map<Room>(request);
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync(cancellationToken);
        return room.RoomId;
    }
}

