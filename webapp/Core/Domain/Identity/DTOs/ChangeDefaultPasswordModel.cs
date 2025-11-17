using System.ComponentModel.DataAnnotations;

namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;

public class ChangeDefaultPasswordModel
{
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
}