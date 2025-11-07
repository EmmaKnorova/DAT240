using Microsoft.AspNetCore.Identity;
using UiS.Dat240.Lab3.SharedKernel;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users;

public class User : BaseEntity
{
	public Guid Id { get; protected set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<Role> Roles { get; set; } = [];
}