using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    [BindProperty]
    public RegisterInputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<RegisterModel> logger,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            // 1. Create user
            var user = new User
            {
                UserName = Input.UserName,
                Email = Input.Email,
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation($"New user has registered: {Input.Name}.");

                // 2. Assign default role (enum-based)
                var defaultRole = Roles.User.ToString();
                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return Page();
                }

                // 4. Sign in user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}
