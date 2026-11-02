
using IPLMerchandise.Application.Interfaces;

namespace IPLMerchandise.Application.Services;

public sealed class MockPaymentService : IPaymentService
{
    public Task<bool> ProcessPaymentAsync(
        decimal amount,
        string currency,
        string paymentReference,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
            throw new ArgumentException(
                "Payment amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency is required.");

        if (string.IsNullOrWhiteSpace(paymentReference))
            throw new ArgumentException(
                "Payment reference is required.");

        // Simulated payment gateway.
        // Actual success/failure will be controlled by PaymentRequest
        // in the application service.
        return Task.FromResult(true);
    }
}
