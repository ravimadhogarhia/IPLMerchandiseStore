
namespace IPLMerchandise.Domain.Entities;

public sealed class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
