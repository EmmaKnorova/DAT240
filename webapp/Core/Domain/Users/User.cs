using Microsoft.AspNetCore.Identity;

namespace TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;

public class User : IdentityUser
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    
}