
namespace IPLMerchandise.Application.DTOs.Carts;

public sealed class CartDto
{
    public string CustomerId { get; set; } = string.Empty;

    public List<CartItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }

    public string Currency { get; set; } = "INR";
}

public sealed class CartItemDto
{
    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
