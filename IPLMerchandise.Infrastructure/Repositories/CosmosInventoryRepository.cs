
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

            var inventory = MapToDomain(response.Resource);

            // Capture Cosmos DB system ETag.
            inventory.ETag = response.ETag;

            return inventory;
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

        try
        {
            ItemResponse<InventoryDocument> response;

            if (string.IsNullOrWhiteSpace(inventory.ETag))
            {
                // New inventory document.
                response = await _container.CreateItemAsync(
                    document,
                    new PartitionKey(inventory.FranchiseId),
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Update only if the stored ETag still matches.
                response = await _container.ReplaceItemAsync(
                    document,
                    inventory.Id,
                    new PartitionKey(inventory.FranchiseId),
                    new ItemRequestOptions
                    {
                        IfMatchEtag = inventory.ETag
                    },
                    cancellationToken);
            }

            // Store the latest ETag for the next update.
            inventory.ETag = response.ETag;
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed ||
                  ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "Inventory was modified by another request. " +
                "Reload the inventory and retry the operation.",
                ex);
        }
    }

    private static Inventory MapToDomain(
        InventoryDocument document)
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

    private static InventoryDocument MapToDocument(
        Inventory inventory)
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

