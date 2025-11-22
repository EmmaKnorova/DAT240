using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages.Courier.Profile;

[Authorize(Roles = "Courier")]
public class EditProfileModel : PageModel
{
    public User CurrentUser { get; set; }
    private readonly UserManager<User> _userManager;
    
    public EditProfileModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public EditProfileInputModel Input { get; set; }

    public async Task OnGetAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);

        Input = new EditProfileInputModel
        {
            UserName = CurrentUser.UserName,
            Email = CurrentUser.Email,
            Name = CurrentUser.Name,
            PhoneNumber = CurrentUser.PhoneNumber,
            Address = CurrentUser.Address,
            City = CurrentUser.City,
            PostalCode = CurrentUser.PostalCode
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);

        CurrentUser.UserName = Input.UserName;
        CurrentUser.Email = Input.Email;
        CurrentUser.Name = Input.Name;
        CurrentUser.PhoneNumber = Input.PhoneNumber;
        CurrentUser.Address = Input.Address;
        CurrentUser.City = Input.City;
        CurrentUser.PostalCode = Input.PostalCode;

        await _userManager.UpdateAsync(CurrentUser);

        return RedirectToPage("/Courier/Profile/Profile");
    }
}
