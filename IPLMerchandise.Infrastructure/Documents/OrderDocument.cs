
using Newtonsoft.Json;

namespace IPLMerchandise.Infrastructure.Documents;

public sealed class OrderDocument
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("documentType")]
    public string DocumentType { get; set; } = "Order";

    [JsonProperty("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonProperty("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [JsonProperty("items")]
    public List<OrderItemDocument> Items { get; set; } = new();

    [JsonProperty("subtotal")]
    public decimal Subtotal { get; set; }

    [JsonProperty("shippingCharges")]
    public decimal ShippingCharges { get; set; }

    [JsonProperty("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = "INR";

    [JsonProperty("status")]
    public string Status { get; set; } = "Pending";

    [JsonProperty("paymentStatus")]
    public string PaymentStatus { get; set; } = "Pending";

    [JsonProperty("shippingAddress")]
    public ShippingAddressDocument ShippingAddress { get; set; } = new();

    [JsonProperty("idempotencyKey")]
    public string? IdempotencyKey { get; set; }

    [JsonProperty("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    [JsonProperty("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class OrderItemDocument
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

public sealed class ShippingAddressDocument
{
    [JsonProperty("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonProperty("addressLine1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonProperty("addressLine2")]
    public string? AddressLine2 { get; set; }

    [JsonProperty("city")]
    public string City { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = "India";
}
