
namespace IPLMerchandise.Domain.Entities;

public sealed class Inventory
{
    public string Id { get; set; } = string.Empty;

    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public int InStockQuantity =>
        AvailableQuantity - ReservedQuantity;
    public string? ETag { get; set; }
}
