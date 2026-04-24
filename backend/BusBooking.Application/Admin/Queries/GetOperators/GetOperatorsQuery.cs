using BusBooking.Application.Admin.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetOperators;

public record GetOperatorsQuery(OperatorStatus? Status = null) : IRequest<List<OperatorRequestDto>>;

public class GetOperatorsQueryHandler : IRequestHandler<GetOperatorsQuery, List<OperatorRequestDto>>
{
    private readonly IAppDbContext _context;

    public GetOperatorsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OperatorRequestDto>> Handle(GetOperatorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BusOperators.Include(o => o.User).AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        return await query
            .Select(o => new OperatorRequestDto
            {
                Id = o.Id,
                FullName = o.User.FullName,
                Email = o.User.Email,
                Phone = o.User.Phone,
                CompanyName = o.CompanyName,
                LicenseNumber = o.LicenseNumber,
                Address = o.Address,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
