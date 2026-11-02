using IPLMerchandise.Application.DTOs.Orders;

namespace IPLMerchandise.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(
        string customerId,
        CheckoutRequest request,
        CancellationToken cancellationToken);

    Task<OrderDto> GetOrderAsync(
        string customerId,
        string orderId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderDto>> GetCustomerOrdersAsync(
        string customerId,
        CancellationToken cancellationToken);

    Task<OrderDto> ProcessPaymentAsync(
    string customerId,
    string orderId,
    PaymentRequest request,
    CancellationToken cancellationToken);
}