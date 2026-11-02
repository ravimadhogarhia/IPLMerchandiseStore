namespace IPLMerchandise.Application.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency,
        string paymentReference,
        CancellationToken cancellationToken);
}