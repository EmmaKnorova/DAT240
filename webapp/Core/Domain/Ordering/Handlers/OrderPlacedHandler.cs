using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Events;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Handlers;

public class OrderPlacedHandler : INotificationHandler<OrderPlaced>
{
    private readonly ShopContext _db;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;

    public OrderPlacedHandler(
        ShopContext db, 
        INotificationService notificationService,
        UserManager<User> userManager)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task Handle(OrderPlaced notification, CancellationToken cancellationToken)
    {
        var couriers = await _userManager.GetUsersInRoleAsync("Courier");
        var approvedCouriers = couriers.Where(c => c.AccountState == AccountStates.Approved);

        foreach (var courier in approvedCouriers)
        {
            await _notificationService.SendNewOrderNotification(courier.Id, notification.orderId);
        }
    }
}