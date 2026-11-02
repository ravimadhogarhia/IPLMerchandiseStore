using IPLMerchandise.Application.DTOs;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;
using IPLMerchandise.Infrastructure.Configuration;
using IPLMerchandise.Infrastructure.Documents;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace IPLMerchandise.Infrastructure.Repositories;

public sealed class CosmosProductRepository : IProductRepository
{
    private readonly Container _container;

    public CosmosProductRepository(
    CosmosClient client,
    IOptions<CosmosDbOptions> options)
    {
        _container = client.GetContainer(
            options.Value.DatabaseName,
            "Products");
    }

    public async Task<Product?> GetByIdAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<ProductDocument>(
                productId,
                new PartitionKey(franchiseId),
                cancellationToken: cancellationToken);

            return Map(response.Resource);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<PagedResult<Product>> SearchAsync(
    string? search,
    string? franchiseId,
    ProductCategory? category,
    int pageSize,
    string? continuationToken,
    CancellationToken cancellationToken)
    {
        var conditions = new List<string>
    {
        "c.documentType = @documentType",
        "c.isActive = true"
    };

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.documentType = @documentType AND c.isActive = true")
            .WithParameter("@documentType", "Product");

        if (!string.IsNullOrWhiteSpace(franchiseId))
        {
            conditions.Add("c.franchiseId = @franchiseId");
        }

        if (category.HasValue)
        {
            conditions.Add("c.category = @category");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(
                "(CONTAINS(c.name, @search, true) OR CONTAINS(c.description, @search, true))");
        }

        // Build the final Cosmos DB query
        var sql = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        query = new QueryDefinition(sql)
            .WithParameter("@documentType", "Product");

        if (!string.IsNullOrWhiteSpace(franchiseId))
        {
            query = query.WithParameter("@franchiseId", franchiseId);
        }

        if (category.HasValue)
        {
            query = query.WithParameter("@category", category.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.WithParameter("@search", search.Trim());
        }

        var queryOptions = new QueryRequestOptions
        {
            MaxItemCount = pageSize
        };

        using var iterator = _container.GetItemQueryIterator<ProductDocument>(
            queryDefinition: query,
            continuationToken: continuationToken,
            requestOptions: queryOptions);

        if (!iterator.HasMoreResults)
        {
            return new PagedResult<Product>
            {
                Items = Array.Empty<Product>(),
                ContinuationToken = null,
                PageSize = pageSize
            };
        }

        var response = await iterator.ReadNextAsync(cancellationToken);

        var products = response
            .Select(Map)
            .ToList();

        return new PagedResult<Product>
        {
            Items = products,
            ContinuationToken = response.ContinuationToken,
            PageSize = pageSize
        };
    }

    private static Product Map(ProductDocument document)
    {
        return new Product
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description,
            FranchiseId = document.FranchiseId,
            FranchiseName = document.FranchiseName,
            Category = document.Category,
            Price = document.Price,
            Currency = document.Currency,
            ImageUrl = document.ImageUrl,
            IsActive = document.IsActive,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }
}
