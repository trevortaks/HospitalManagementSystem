using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Billing.Queries.GetById;

public class GetBillingByIdQueryHandler : IRequestHandler<GetBillingByIdQuery, BillingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBillingByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BillingDto> Handle(GetBillingByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills.AsNoTracking()
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BillingId == request.BillingId, cancellationToken);

        if (bill == null)
        {
            throw new NotFoundException("Billing", request.BillingId);
        }

        return _mapper.Map<BillingDto>(bill);
    }
}
