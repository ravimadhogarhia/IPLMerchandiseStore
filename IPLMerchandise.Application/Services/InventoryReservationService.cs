using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Services;

public sealed class InventoryReservationService
    : IInventoryReservationService
{
    private const int MaxRetries = 5;

    private readonly IInventoryRepository _inventoryRepository;

    public InventoryReservationService(
        IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task ReserveAsync(
        string franchiseId,
        string productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        ValidateRequest(franchiseId, productId, quantity);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var inventory = await _inventoryRepository.GetByProductAsync(
                franchiseId,
                productId,
                cancellationToken);

            if (inventory is null)
                throw new KeyNotFoundException(
                    $"Inventory not found for product {productId}.");

            ValidateInventory(inventory);

            if (inventory.InStockQuantity < quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product {productId}.");

            inventory.ReservedQuantity += quantity;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                await _inventoryRepository.UpsertAsync(
                    inventory,
                    cancellationToken);

                return;
            }
            catch (InvalidOperationException ex)
                when (ex.Message.StartsWith(
                    "Inventory was modified",
                    StringComparison.Ordinal))
            {
                if (attempt == MaxRetries)
                    throw;
            }
        }

        throw new InvalidOperationException(
            "Unable to reserve inventory after multiple attempts.");
    }

    public async Task ReleaseAsync(
        string franchiseId,
        string productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        ValidateRequest(franchiseId, productId, quantity);

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var inventory = await _inventoryRepository.GetByProductAsync(
                franchiseId,
                productId,
                cancellationToken);

            if (inventory is null)
                throw new KeyNotFoundException(
                    $"Inventory not found for product {productId}.");

            ValidateInventory(inventory);

            if (inventory.ReservedQuantity < quantity)
                throw new InvalidOperationException(
                    $"Cannot release more stock than reserved for {productId}.");

            inventory.ReservedQuantity -= quantity;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            try
            {
                await _inventoryRepository.UpsertAsync(
                    inventory,
                    cancellationToken);

                return;
            }
            catch (InvalidOperationException ex)
                when (ex.Message.StartsWith(
                    "Inventory was modified",
                    StringComparison.Ordinal))
            {
                if (attempt == MaxRetries)
                    throw;
            }
        }

        throw new InvalidOperationException(
            "Unable to release inventory after multiple attempts.");
    }

    private static void ValidateRequest(
        string franchiseId,
        string productId,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(franchiseId))
            throw new ArgumentException("Franchise ID is required.");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required.");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");
    }

    private static void ValidateInventory(Inventory inventory)
    {
        if (inventory.AvailableQuantity < 0 ||
            inventory.ReservedQuantity < 0 ||
            inventory.ReservedQuantity > inventory.AvailableQuantity)
        {
            throw new InvalidOperationException(
                "Inventory data is inconsistent.");
        }
    }
}