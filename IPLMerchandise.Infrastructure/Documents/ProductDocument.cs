
using IPLMerchandise.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IPLMerchandise.Infrastructure.Documents;

public sealed class ProductDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("documentType")]
    public string DocumentType { get; set; } = "Product";

    [JsonProperty("franchiseId")]
    public string FranchiseId { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("franchiseName")]
    public string FranchiseName { get; set; } = string.Empty;

    [JsonProperty("category")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ProductCategory Category { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = "INR";

    [JsonProperty("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonProperty("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    [JsonProperty("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }
}
