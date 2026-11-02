
using IPLMerchandise.Application.DTOs.Carts;

namespace IPLMerchandise.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(
        string customerId,
        CancellationToken cancellationToken);

    Task<CartDto> AddItemAsync(
        string customerId,
        AddToCartRequest request,
        CancellationToken cancellationToken);

    Task<CartDto> UpdateItemAsync(
    string customerId,
    string franchiseId,
    string productId,
    UpdateCartItemRequest request,
    CancellationToken cancellationToken);

    Task RemoveItemAsync(
        string customerId,
        string franchiseId,
        string productId,
        CancellationToken cancellationToken);
}
