
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Infrastructure.Documents;
using Microsoft.Azure.Cosmos;

namespace IPLMerchandise.Infrastructure.Repositories;

public sealed class CosmosCartRepository : ICartRepository
{
    private readonly Container _container;

    public CosmosCartRepository(CosmosClient client)
    {
        _container = client.GetContainer(
            "IplMerchandiseDb",
            "Carts");
    }

    public async Task<Cart?> GetByCustomerIdAsync(
    string customerId,
    CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<CartDocument>(
                customerId,
                new PartitionKey(customerId),
                cancellationToken: cancellationToken);

            var cart = MapToDomain(response.Resource);

            // Capture Cosmos DB document version.
            cart.ETag = response.ETag;

            return cart;
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task UpsertAsync(
    Cart cart,
    CancellationToken cancellationToken)
    {
        var document = MapToDocument(cart);

        try
        {
            ItemResponse<CartDocument> response;

            if (string.IsNullOrWhiteSpace(cart.ETag))
            {
                // First save: create only.
                // This prevents two simultaneous first-time requests
                // from silently overwriting each other.
                response = await _container.CreateItemAsync(
                    document,
                    new PartitionKey(cart.CustomerId),
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Existing cart: replace only if its version is unchanged.
                response = await _container.ReplaceItemAsync(
                    document,
                    cart.Id,
                    new PartitionKey(cart.CustomerId),
                    new ItemRequestOptions
                    {
                        IfMatchEtag = cart.ETag
                    },
                    cancellationToken);
            }

            // Keep the latest version for subsequent updates.
            cart.ETag = response.ETag;
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.Conflict ||
                  ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            throw new InvalidOperationException(
                "Your cart was modified by another request. Please reload the cart and try again.",
                ex);
        }
    }

    private static Cart MapToDomain(CartDocument document)
    {
        return new Cart
        {
            Id = document.Id,
            CustomerId = document.CustomerId,
            Currency = document.Currency,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc,

            Items = document.Items.Select(item => new CartItem
            {
                ProductId = item.ProductId,
                FranchiseId = item.FranchiseId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }

    private static CartDocument MapToDocument(Cart cart)
    {
        return new CartDocument
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Currency = cart.Currency,
            CreatedAtUtc = cart.CreatedAtUtc,
            UpdatedAtUtc = cart.UpdatedAtUtc,

            Items = cart.Items.Select(item => new CartItemDocument
            {
                ProductId = item.ProductId,
                FranchiseId = item.FranchiseId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList()
        };
    }
}
