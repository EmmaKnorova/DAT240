using System.ComponentModel.DataAnnotations;

namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;

public class RegisterInputModel
{
    [Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]  
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]  
    [Display(Name = "Confirm password")]  
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]  
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}