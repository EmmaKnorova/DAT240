using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.Pages.Identity;

[AllowAnonymous]
public class LoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginModel> logger,
    RoleManager<IdentityRole<Guid>> roleManager) : PageModel
{
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<LoginModel> _logger = logger;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
    [BindProperty]
    public LoginInputModel Input { get; set; }
    public List<string> AvailableRoles { get; set; } = ["User", "Courier"];

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        Console.WriteLine($"It works! {Input.Email} and {Input.Password}");

        var user = await _userManager.FindByEmailAsync(Input.Email);
        Console.WriteLine(user);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            Console.WriteLine("User not found!");
            return Page();
        }
        
        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );
        
        if (result.Succeeded)
        {
            _logger.LogInformation($"User logged in: {user.Email}");
            return Redirect("/");
        }
        
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}
