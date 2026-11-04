
using IPLMerchandise.Application.DTOs.Auth;

namespace IPLMerchandise.Application.Interfaces;

public interface IAuthService
{
    Task<AuthenticatedUserDto> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken);

    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);
}
