
namespace IPLMerchandise.Application.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAtUtc { get; set; }

    public AuthenticatedUserDto User { get; set; } = new();
}
