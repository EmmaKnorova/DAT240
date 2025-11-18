using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.Entities;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

[Authorize(Roles = "Admin")]
public class CouriersOverviewModel(IMediator mediator, UserManager<User> userManager, ShopContext context) : PageModel
{
    public List<User> ApprovedCouriers { get; set; } = [];
    public List<User> PendingCouriers { get; set; } = [];
    public List<User> DeclinedCouriers { get; set; } = [];
    private readonly IMediator _mediator = mediator;
    private readonly ShopContext _context = context;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<IActionResult> OnGetAsync()
    {
        List<User> couriers = (List<User>) await _userManager.GetUsersInRoleAsync("Courier");
        ApprovedCouriers = couriers.Where(c => c.AccountState == AccountStates.Approved).ToList();
        PendingCouriers = couriers.Where(c => c.AccountState == AccountStates.Pending).ToList();
        DeclinedCouriers = couriers.Where(c => c.AccountState == AccountStates.Declined).ToList();
        return Page();
    }

    public async Task OnPostAsync(string userId, bool approve)
    {
        if (approve)
            await _mediator.Send(new Approve.Request(userId));
        else
            await _mediator.Send(new Decline.Request(userId));
        await OnGetAsync();
    }
}
