using System.ComponentModel.DataAnnotations;

namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;

public class SignupModel
{
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}