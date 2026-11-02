
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace IPLMerchandise.Tests.Configuration;

public sealed class CosmosIntegrationTestFixture : IAsyncLifetime
{
    public CosmosClient Client { get; private set; } = null!;

    public Database Database { get; private set; } = null!;

    public const string DatabaseName = "IplMerchandiseIntegrationTests";

    public async Task InitializeAsync()
    {
        var endpoint = "https://localhost:8081/";

        // Read the emulator key from an environment variable.
        var key = Environment.GetEnvironmentVariable(
            "COSMOS_EMULATOR_KEY");

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Set the COSMOS_EMULATOR_KEY environment variable.");
        }

        Client = new CosmosClient(
            endpoint,
            key,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });

        var response = await Client.CreateDatabaseIfNotExistsAsync(
            DatabaseName);

        Database = response.Database;

        await Database.CreateContainerIfNotExistsAsync(
            "Products", "/franchiseId");

        await Database.CreateContainerIfNotExistsAsync(
            "Inventory", "/franchiseId");

        await Database.CreateContainerIfNotExistsAsync(
            "Carts", "/customerId");

        await Database.CreateContainerIfNotExistsAsync(
            "Orders", "/customerId");
    }

    public Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }
}
