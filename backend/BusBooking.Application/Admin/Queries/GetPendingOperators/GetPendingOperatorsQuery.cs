using BusBooking.Application.Admin.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetPendingOperators;

public record GetPendingOperatorsQuery : IRequest<List<OperatorRequestDto>>;

public class GetPendingOperatorsQueryHandler : IRequestHandler<GetPendingOperatorsQuery, List<OperatorRequestDto>>
{
    private readonly IAppDbContext _context;

    public GetPendingOperatorsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OperatorRequestDto>> Handle(GetPendingOperatorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BusOperators
            .Include(o => o.User)
            .Where(o => o.Status == OperatorStatus.Pending)
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
