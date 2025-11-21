using System.ComponentModel.DataAnnotations;

namespace TarlBreuJacoBaraKnor.Core.Domain.Users.DTOs;

public class ChangeDefaultPasswordModel
{
    public string Password { get; set; } = string.Empty;
    [Display(Name = "Confirm Password")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}