using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Application.Interfaces;

namespace IPLMerchandise.Api.Diagnostics;

public static class CartConcurrencyTest
{
    public static async Task RunAsync(
        ICartRepository repository,
        CancellationToken cancellationToken = default)
    {
        const string customerId = "CONCURRENCY-TEST-001";

        // Create the initial cart.
        var initialCart = new Cart
        {
            Id = customerId,
            CustomerId = customerId,
            Currency = "INR",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await repository.UpsertAsync(initialCart, cancellationToken);

        // Simulate two requests reading the same version.
        var requestA = await repository.GetByCustomerIdAsync(
            customerId, cancellationToken);

        var requestB = await repository.GetByCustomerIdAsync(
            customerId, cancellationToken);

        if (requestA is null || requestB is null)
            throw new Exception("Cart could not be loaded.");

        Console.WriteLine($"Request A ETag: {requestA.ETag}");
        Console.WriteLine($"Request B ETag: {requestB.ETag}");

        // Request A updates successfully.
        requestA.Currency = "INR";
        requestA.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpsertAsync(requestA, cancellationToken);

        Console.WriteLine("Request A update succeeded.");

        // Request B still has the old ETag.
        try
        {
            requestB.UpdatedAtUtc = DateTime.UtcNow.AddSeconds(1);

            await repository.UpsertAsync(requestB, cancellationToken);

            Console.WriteLine("ERROR: Stale ETag was not rejected.");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("SUCCESS: Stale ETag rejected.");
            Console.WriteLine(ex.Message);
        }
    }
}