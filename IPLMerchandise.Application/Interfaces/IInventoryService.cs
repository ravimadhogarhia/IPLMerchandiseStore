using IPLMerchandise.Application.DTOs.Inventory;

namespace IPLMerchandise.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryDto> GetInventoryAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken);
}