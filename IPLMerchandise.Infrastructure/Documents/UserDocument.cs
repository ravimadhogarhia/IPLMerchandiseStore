using Newtonsoft.Json;

namespace IPLMerchandise.Infrastructure.Documents;

public sealed class UserDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("documentType")]
    public string DocumentType { get; set; } = "User";

    [JsonProperty("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("normalizedEmail")]
    public string NormalizedEmail { get; set; } = string.Empty;

    [JsonProperty("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonProperty("isActive")]
    public bool IsActive { get; set; }

    [JsonProperty("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }
}