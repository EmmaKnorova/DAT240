using Microsoft.AspNetCore.Identity;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users;

public class User : IdentityUser
{
	public Guid Id { get; protected set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Roles> Roles { get; set; } = [];
}