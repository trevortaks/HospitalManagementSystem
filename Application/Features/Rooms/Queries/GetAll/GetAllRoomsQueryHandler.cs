using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Rooms.Queries.GetAll;

public class GetAllRoomsQueryHandler : IRequestHandler<GetAllRoomsQuery, List<RoomDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllRoomsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<RoomDto>> Handle(GetAllRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _context.Rooms.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<RoomDto>>(rooms);
    }
}

