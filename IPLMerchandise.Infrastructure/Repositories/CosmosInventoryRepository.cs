
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Infrastructure.Documents;
using Microsoft.Azure.Cosmos;

namespace IPLMerchandise.Infrastructure.Repositories;

public sealed class CosmosInventoryRepository : IInventoryRepository
{
    private readonly Container _container;

    public CosmosInventoryRepository(CosmosClient client)
    {
        _container = client.GetContainer(
            "IplMerchandiseDb",
            "Inventory");
    }

    public async Task<Inventory?> GetByProductAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        var id = $"{franchiseId}-{productId}";

        try
        {
            var response = await _container.ReadItemAsync<InventoryDocument>(
                id,
                new PartitionKey(franchiseId),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpsertAsync(
        Inventory inventory,
        CancellationToken cancellationToken)
    {
        var document = MapToDocument(inventory);

        await _container.UpsertItemAsync(
            document,
            new PartitionKey(inventory.FranchiseId),
            cancellationToken: cancellationToken);
    }

    private static Inventory MapToDomain(InventoryDocument document)
    {
        return new Inventory
        {
            Id = document.Id,
            ProductId = document.ProductId,
            FranchiseId = document.FranchiseId,
            AvailableQuantity = document.AvailableQuantity,
            ReservedQuantity = document.ReservedQuantity,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }

    private static InventoryDocument MapToDocument(Inventory inventory)
    {
        return new InventoryDocument
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            FranchiseId = inventory.FranchiseId,
            AvailableQuantity = inventory.AvailableQuantity,
            ReservedQuantity = inventory.ReservedQuantity,
            UpdatedAtUtc = inventory.UpdatedAtUtc
        };
    }
}
