
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByProductAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken);

    Task UpsertAsync(
        Inventory inventory,
        CancellationToken cancellationToken);
}
