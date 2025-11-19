using System.ComponentModel.DataAnnotations;

namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;

public class EditProfileInputModel
{
    [Display(Name = "User name")]
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    [Display(Name = "Postal code")]
    public string PostalCode { get; set; } = string.Empty;
  
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
}