using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Pages.Admin;

public class CouriersOverviewModel(IMediator mediator, UserManager<User> userManager, ShopContext context) : PageModel
{
    public List<User> ApprovedCouriers { get; set; } = [];
    public List<User> PendingCouriers { get; set; } = [];
    private readonly IMediator _mediator = mediator;
    private readonly ShopContext _context = context;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<IActionResult> OnGetAsync()
    {
        List<User> couriers = (List<User>) await _userManager.GetUsersInRoleAsync("Courier");

        foreach (User user in couriers)
        {
            
        }
        return Page();
    }
}