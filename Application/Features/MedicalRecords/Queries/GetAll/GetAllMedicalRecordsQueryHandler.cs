using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.MedicalRecords.Queries.GetAll;

public class GetAllMedicalRecordsQueryHandler : IRequestHandler<GetAllMedicalRecordsQuery, List<MedicalRecordDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllMedicalRecordsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MedicalRecordDto>> Handle(GetAllMedicalRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _context.MedicalRecords.AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<MedicalRecordDto>>(records);
    }
}
