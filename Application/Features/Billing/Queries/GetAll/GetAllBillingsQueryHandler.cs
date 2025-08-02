using AutoMapper;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalManagementSystem.Application.Features.Billing.Queries.GetAll;

public class GetAllBillingsQueryHandler : IRequestHandler<GetAllBillingsQuery, List<BillingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllBillingsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BillingDto>> Handle(GetAllBillingsQuery request, CancellationToken cancellationToken)
    {
        var bills = await _context.Bills.AsNoTracking()
            .Include(b => b.Items)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BillingDto>>(bills);
    }
}
