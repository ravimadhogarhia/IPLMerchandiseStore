
namespace IPLMerchandise.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
