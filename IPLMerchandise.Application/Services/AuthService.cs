using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using IPLMerchandise.Application.DTOs.Auth;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthenticatedUserDto> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.");

        if (string.IsNullOrWhiteSpace(password) ||
            Encoding.UTF8.GetByteCount(password) < 8 ||
            Encoding.UTF8.GetByteCount(password) > 72)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters and no more than 72 UTF-8 bytes.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(
            email, cancellationToken);

        if (existingUser is not null)
            throw new InvalidOperationException(
                "An account with this email already exists.");

        // Deterministic ID prevents duplicate accounts during concurrent registration.
        var userId = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(email)))
            .ToLowerInvariant();

        var user = new User
        {
            Id = userId,
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);

        return MapToDto(user);
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email, cancellationToken);

        if (user is null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) ||
            !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var tokenResult = _jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            AccessToken = tokenResult.AccessToken,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            User = MapToDto(user)
        };
    }

    private static AuthenticatedUserDto MapToDto(User user)
    {
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }
}