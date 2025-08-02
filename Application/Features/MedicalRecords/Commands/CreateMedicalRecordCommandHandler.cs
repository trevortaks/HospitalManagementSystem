namespace HospitalManagementSystem.Application.Features.MedicalRecords.Commands;

using MediatR;
using AutoMapper;
using Application.Common.Interfaces; // Adjust namespace for your unit of work or dbcontext
using Domain.Entities;

public class CreateMedicalRecordCommandHandler : Common.Interfaces.IRequestHandler<CreateMedicalRecordCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateMedicalRecordCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<MedicalRecord>(request);

        _context.MedicalRecords.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.RecordId;
    }
}
