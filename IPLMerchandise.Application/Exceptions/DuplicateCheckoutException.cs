namespace IPLMerchandise.Application.Exceptions;

public sealed class DuplicateCheckoutException : Exception
{
    public DuplicateCheckoutException(
        string customerId,
        string idempotencyKey,
        Exception? innerException = null)
        : base("This checkout request has already been processed.",
            innerException)
    {
        CustomerId = customerId;
        IdempotencyKey = idempotencyKey;
    }

    public string CustomerId { get; }

    public string IdempotencyKey { get; }
}