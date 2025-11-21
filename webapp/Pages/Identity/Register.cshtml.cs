using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class RegisterModel(
    IMediator mediator,
    UserManager<User> userManager,
    SignInManager<User> signInManager
    ) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IMediator _mediator = mediator;
    [BindProperty]
    public required RegisterInputModel Input { get; set; }
    public List<string> AvailableRoles { get; set; } = [Roles.Customer.ToString(), Roles.Courier.ToString()];

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity.IsAuthenticated)
            return Redirect("/");
        else return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {

        if (!ModelState.IsValid)
            return Page();

        var result = await _mediator.Send(new RegisterInternalUser.Request(Input));

        if (result.IsSuccess)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);

            if (Input.Role == "Customer")
                return LocalRedirect("/Customer/Menu");
            return LocalRedirect("/Courier/Dashboard");
        }
        else
        {
            Console.WriteLine("Error!");
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return Page();
        }
    }
}
