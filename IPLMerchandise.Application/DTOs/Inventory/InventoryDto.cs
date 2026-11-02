namespace IPLMerchandise.Application.DTOs.Inventory;

public sealed class InventoryDto
{
    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int InStockQuantity { get; set; }

    public bool IsInStock => InStockQuantity > 0;
}