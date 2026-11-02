
using IPLMerchandise.Infrastructure.Configuration;
using IPLMerchandise.Infrastructure.Documents;
using IPLMerchandise.Infrastructure.Seed;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;

namespace IPLMerchandise.Infrastructure.Persistence;

public sealed class CosmosDbInitializer : IHostedService
{
    private readonly CosmosClient _client;
    private readonly CosmosDbOptions _options;

    public CosmosDbInitializer(
        CosmosClient client,
        IOptions<CosmosDbOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        var response = await _client.CreateDatabaseIfNotExistsAsync(
            _options.DatabaseName,
            cancellationToken: cancellationToken);

        var database = response.Database;

        var containers = new[]
        {
            ("Products", "/franchiseId"),
            ("Inventory", "/franchiseId"),
            ("Carts", "/customerId"),
            ("Orders", "/customerId")
        };

        foreach (var (name, partitionKey) in containers)
        {
            await database.CreateContainerIfNotExistsAsync(
                name,
                partitionKey,
                cancellationToken: cancellationToken);
        }

        await SeedProductsAsync(
            _client.GetContainer(_options.DatabaseName, "Products"),cancellationToken);

        var inventoryContainer = database.GetContainer("Inventory");

        await SeedInventoryAsync(
            inventoryContainer,
            cancellationToken);
    }

    private async Task SeedProductsAsync(
    Container container,
    CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT TOP 1 c.id FROM c WHERE c.documentType = @type")
            .WithParameter("@type", "Product");

        using var iterator =
            container.GetItemQueryIterator<dynamic>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            if (response.Any())
                return;
        }

        foreach (var product in ProductSeedData.GetProducts())
        {
            await container.CreateItemAsync(
                product,
                new PartitionKey(product.FranchiseId),
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedInventoryAsync(
    Container inventoryContainer,
    CancellationToken cancellationToken)
    {
        var records = new[]
        {
        new { FranchiseId = "MI", ProductId = "MI-JERSEY-001" },
        new { FranchiseId = "CSK", ProductId = "CSK-CAP-001" },
        new { FranchiseId = "RCB", ProductId = "RCB-FLAG-001" },
        new { FranchiseId = "KKR", ProductId = "KKR-PHOTO-001" },
        new { FranchiseId = "SRH", ProductId = "SRH-JERSEY-001" },
        new { FranchiseId = "RR", ProductId = "RR-CAP-001" },
        new { FranchiseId = "DC", ProductId = "DC-JERSEY-001" },
        new { FranchiseId = "GT", ProductId = "GT-FLAG-001" }
    };

        foreach (var record in records)
        {
            var document = new InventoryDocument
            {
                Id = $"{record.FranchiseId}-{record.ProductId}",
                DocumentType = "Inventory",
                FranchiseId = record.FranchiseId,
                ProductId = record.ProductId,
                AvailableQuantity = 100,
                ReservedQuantity = 0,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await inventoryContainer.UpsertItemAsync(
                document,
                new PartitionKey(record.FranchiseId),
                cancellationToken: cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
