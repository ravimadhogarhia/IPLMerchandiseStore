using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        string customerId,
        string orderId,
        CancellationToken cancellationToken);

    Task<Order?> GetByIdempotencyKeyAsync(
        string customerId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task CreateAsync(
        Order order,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken);
    Task UpdateAsync(
    Order order,
    CancellationToken cancellationToken);
}