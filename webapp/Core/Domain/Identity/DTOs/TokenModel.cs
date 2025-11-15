namespace TarlBreuJacoBaraKnor.Core.Domain.Identity.DTOs;

public class TokenModel
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}