
using IPLMerchandise.Infrastructure.Configuration;
using IPLMerchandise.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Infrastructure.Repositories;

namespace IPLMerchandise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(CosmosDbOptions.SectionName)
            .Get<CosmosDbOptions>()
            ?? throw new InvalidOperationException(
                "Cosmos DB configuration is missing.");

        if (string.IsNullOrWhiteSpace(options.Endpoint) ||
            string.IsNullOrWhiteSpace(options.Key) ||
            string.IsNullOrWhiteSpace(options.DatabaseName))
        {
            throw new InvalidOperationException(
                "Cosmos DB configuration is incomplete.");
        }

        services.Configure<CosmosDbOptions>(
            configuration.GetSection(CosmosDbOptions.SectionName));
        services.AddScoped<IProductRepository, CosmosProductRepository>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartRepository, CosmosCartRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IInventoryRepository, CosmosInventoryRepository>();
        services.AddScoped<IInventoryService, InventoryService>();

        services.AddSingleton(_ => new CosmosClient(
            options.Endpoint,
            options.Key,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy =
                        CosmosPropertyNamingPolicy.CamelCase
                }
            }));

        services.AddHostedService<CosmosDbInitializer>();

        return services;
    }
}
