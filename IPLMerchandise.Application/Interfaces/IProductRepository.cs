using IPLMerchandise.Application.DTOs;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken);

    Task<PagedResult<Product>> SearchAsync(
        string? search,
        string? franchiseId,
        ProductCategory? category,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
}