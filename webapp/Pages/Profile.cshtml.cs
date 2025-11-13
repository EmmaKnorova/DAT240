using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

namespace TarlBreuJacoBaraKnor.webapp.Pages
{
    public class ProfileModel : PageModel
    {

        private readonly UserManager<User> _userManager;
        public User CurrentUser;

        public ProfileModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
         
        [BindProperty]
        public string PhoneNumber { get; set; } = "";
        [BindProperty]
        public string Name { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            CurrentUser = user;
            return Page();
        }


    }
}
