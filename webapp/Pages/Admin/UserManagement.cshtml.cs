using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MediatR;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users.Pipelines;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class UserManagementModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IMediator _mediator;

    public UserManagementModel(UserManager<User> userManager, IMediator mediator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public List<UserViewModel> Users { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    public class UserViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccountState { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var allUsers = await _userManager.Users.ToListAsync();

        var userViewModels = new List<UserViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "No Role";

            if (!string.IsNullOrEmpty(RoleFilter) && role != RoleFilter)
            {
                continue;
            }

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? "N/A",
                Role = role,
                AccountState = user.AccountState.ToString()
            });
        }

        Users = userViewModels.OrderBy(u => u.Name).ToList();
    }

    public async Task<IActionResult> OnPostPromoteToAdminAsync(Guid userId)
    {
        var currentUserId = Guid.Parse(_userManager.GetUserId(User)!);
        if (userId == currentUserId)
        {
            TempData["ErrorMessage"] = "You cannot modify your own role.";
            return RedirectToPage();
        }

        var response = await _mediator.Send(new InviteToAdmin.Request(userId));

        if (response.Success)
        {
            TempData["SuccessMessage"] = "User successfully promoted to Administrator!";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", response.Errors);
        }

        return RedirectToPage(new { RoleFilter });
    }
}