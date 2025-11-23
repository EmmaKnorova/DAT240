using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Pipelines;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class LoginModel(
    IExternalAuthService authService,
    IMediator mediator) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty] public required LoginInputModel Input { get; set; }
    private readonly IExternalAuthService _authService = authService;
    private readonly IMediator _mediator = mediator;
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!User.Identity.IsAuthenticated)
            return Page();

        if (User.IsInRole(Roles.Customer.ToString()))
            return Redirect("/Customer/Menu");
        else if (User.IsInRole(Roles.Courier.ToString()))
            return Redirect("/Courier/Dashboard");
        return Redirect("/");
    }

    public IActionResult OnPostExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Page("/Identity/ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _authService.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        
        var command = new LoginInputModel
        {
            Email = Input.Email,
            Password = Input.Password
        };

        var result = await _mediator.Send(new LogInInternalUser.Request(command)); 

        if (result.IsSuccess)
        {
            var roleBasedPath = result.Value; 
            
            if (roleBasedPath == "/" && !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }
            return LocalRedirect(roleBasedPath!); 
        }

        if (result.Errors.Contains("Account is locked. Try again later."))
        {
            ModelState.AddModelError(string.Empty, "Account is locked. Try again later.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                 ModelState.AddModelError(string.Empty, error);
            }
        }
        
        return Page();
    }
}