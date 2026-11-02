using IPLMerchandise.Application.DTOs;
using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken);

    Task<PagedResult<ProductDto>> SearchAsync(
        string? search,
        string? franchiseId,
        ProductCategory? category,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
}