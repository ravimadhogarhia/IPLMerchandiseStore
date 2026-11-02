using Newtonsoft.Json;

namespace IPLMerchandise.Infrastructure.Documents;

public sealed class CartDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("documentType")]
    public string DocumentType { get; set; } = "Cart";

    [JsonProperty("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonProperty("items")]
    public List<CartItemDocument> Items { get; set; } = new();

    [JsonProperty("currency")]
    public string Currency { get; set; } = "INR";

    [JsonProperty("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    [JsonProperty("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class CartItemDocument
{
    [JsonProperty("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("franchiseId")]
    public string FranchiseId { get; set; } = string.Empty;

    [JsonProperty("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonProperty("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonProperty("unitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }
}