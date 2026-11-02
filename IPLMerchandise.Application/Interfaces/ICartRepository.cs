
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken);

    Task UpsertAsync(
        Cart cart,
        CancellationToken cancellationToken);
}
