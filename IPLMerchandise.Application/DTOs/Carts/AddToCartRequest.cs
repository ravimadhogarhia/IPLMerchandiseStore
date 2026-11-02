
namespace IPLMerchandise.Application.DTOs.Carts;

public sealed class AddToCartRequest
{
    public string FranchiseId { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
