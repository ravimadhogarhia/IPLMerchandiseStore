
namespace IPLMerchandise.Domain.Entities;

public sealed class Cart
{
    public string Id { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public List<CartItem> Items { get; set; } = new();

    public string Currency { get; set; } = "INR";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
    public string? ETag { get; set; }
}

public sealed class CartItem
{
    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
