using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Rooms.Queries.GetById;

public class GetRoomByIdQueryHandler : Common.Interfaces.IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRoomByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == request.RoomId, cancellationToken);

        if (room == null)
        {
            throw new NotFoundException("Room", request.RoomId);
        }

        return _mapper.Map<RoomDto>(room);
    }
}

