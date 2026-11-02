using IPLMerchandise.Application.DTOs.Inventory;
using IPLMerchandise.Application.Interfaces;

namespace IPLMerchandise.Application.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<InventoryDto> GetInventoryAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(franchiseId))
            throw new ArgumentException("Franchise ID is required.");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required.");

        var inventory = await _inventoryRepository.GetByProductAsync(
            franchiseId.Trim().ToUpperInvariant(),
            productId.Trim(),
            cancellationToken);

        if (inventory is null)
            throw new KeyNotFoundException("Inventory record not found.");

        if (inventory.AvailableQuantity < 0 ||
            inventory.ReservedQuantity < 0 ||
            inventory.ReservedQuantity > inventory.AvailableQuantity)
        {
            throw new InvalidOperationException(
                "Inventory data is invalid.");
        }

        return new InventoryDto
        {
            ProductId = inventory.ProductId,
            FranchiseId = inventory.FranchiseId,
            AvailableQuantity = inventory.AvailableQuantity,
            ReservedQuantity = inventory.ReservedQuantity,
            InStockQuantity = inventory.InStockQuantity
        };
    }
}