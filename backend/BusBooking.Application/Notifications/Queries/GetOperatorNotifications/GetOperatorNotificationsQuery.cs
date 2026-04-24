using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Notifications.Queries.GetOperatorNotifications;

public record GetOperatorNotificationsQuery(Guid OperatorId, bool UnreadOnly = false) : IRequest<List<NotificationDto>>;

public class GetOperatorNotificationsQueryHandler : IRequestHandler<GetOperatorNotificationsQuery, List<NotificationDto>>
{
    private readonly IAppDbContext _context;

    public GetOperatorNotificationsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDto>> Handle(GetOperatorNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == request.OperatorId);

        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                RelatedBookingId = n.RelatedBookingId,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
