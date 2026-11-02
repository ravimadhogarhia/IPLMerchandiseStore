

using IPLMerchandise.Infrastructure.Configuration;
using IPLMerchandise.Infrastructure.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace IPLMerchandise.Tests.Integration;

public class ProductRepositoryIntegrationTests
{
    private readonly CosmosClient _client;
    private readonly CosmosProductRepository _repository;
    private readonly Container _container;
    private readonly ITestOutputHelper _output;

    private const string DatabaseName =
        "IplMerchandiseIntegrationTests";

    public ProductRepositoryIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Set COSMOS_EMULATOR_KEY before running integration tests.");
        }

        _client = new CosmosClient(
            "https://localhost:8081/",
            key,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });

        var options = Options.Create(new CosmosDbOptions
        {
            Endpoint = "https://localhost:8081/",
            Key = key,
            DatabaseName = DatabaseName
        });

        _repository = new CosmosProductRepository(_client, options);

        _container = _client.GetContainer(
            DatabaseName,
            "Products");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = $"UNKNOWN-{Guid.NewGuid():N}";

        // Act
        var result = await _repository.GetByIdAsync(
            "MI",
            productId,
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    
    public void Dispose()
    {
        _client.Dispose();
    }
}
