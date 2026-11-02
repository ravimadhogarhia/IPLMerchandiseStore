
using Newtonsoft.Json;

namespace IPLMerchandise.Infrastructure.Documents;

public sealed class InventoryDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("documentType")]
    public string DocumentType { get; set; } = "Inventory";

    [JsonProperty("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("franchiseId")]
    public string FranchiseId { get; set; } = string.Empty;

    [JsonProperty("availableQuantity")]
    public int AvailableQuantity { get; set; }

    [JsonProperty("reservedQuantity")]
    public int ReservedQuantity { get; set; }

    [JsonProperty("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }
}
