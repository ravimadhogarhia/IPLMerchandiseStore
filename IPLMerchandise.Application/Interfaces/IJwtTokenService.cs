
using IPLMerchandise.Application.DTOs.Auth;
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(User user);

    //DateTime GetExpiryTimeUtc();
}
