using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.MedicalRecords.Queries.GetById;

public class GetMedicalRecordByIdQueryHandler : Common.Interfaces.IRequestHandler<GetMedicalRecordByIdQuery, MedicalRecordDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMedicalRecordByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MedicalRecordDto> Handle(GetMedicalRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _context.MedicalRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecordId == request.RecordId, cancellationToken);

        if (record == null)
        {
            throw new NotFoundException("MedicalRecord", request.RecordId);
        }

        return _mapper.Map<MedicalRecordDto>(record);
    }
}
