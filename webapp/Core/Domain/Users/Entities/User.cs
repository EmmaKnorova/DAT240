using Microsoft.AspNetCore.Identity;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Entities;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public bool ChangePasswordOnFirstLogin { get; set; } = false;
    public AccountStates AccountState { get; set; } = AccountStates.Pending; 
}