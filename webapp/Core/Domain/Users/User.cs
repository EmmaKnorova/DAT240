using Microsoft.AspNetCore.Identity;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users;

public class User : IdentityUser
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}