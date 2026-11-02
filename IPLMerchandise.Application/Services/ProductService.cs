using IPLMerchandise.Application.DTOs;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductDto?> GetByIdAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(franchiseId))
            throw new ArgumentException("Franchise ID is required.");

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required.");

        var product = await _repository.GetByIdAsync(
            franchiseId.Trim().ToUpperInvariant(),
            productId.Trim(),
            cancellationToken);

        return product is null ? null : Map(product);
    }

    public async Task<PagedResult<ProductDto>> SearchAsync(
        string? search,
        string? franchiseId,
        ProductCategory? category,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException(
                "Page size must be between 1 and 100.");

        if (franchiseId is not null &&
            string.IsNullOrWhiteSpace(franchiseId))
        {
            throw new ArgumentException("Franchise ID cannot be empty.");
        }

        var result = await _repository.SearchAsync(
            search,
            franchiseId?.Trim().ToUpperInvariant(),
            category,
            pageSize,
            continuationToken,
            cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = result.Items.Select(Map).ToList(),
            ContinuationToken = result.ContinuationToken,
            PageSize = result.PageSize
        };
    }

    private static ProductDto Map(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            FranchiseId = product.FranchiseId,
            FranchiseName = product.FranchiseName,
            Category = product.Category,
            Price = product.Price,
            Currency = product.Currency,
            ImageUrl = product.ImageUrl
        };
    }
}