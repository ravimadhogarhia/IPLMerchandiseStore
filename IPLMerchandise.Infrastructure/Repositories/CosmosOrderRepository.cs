
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;
using IPLMerchandise.Infrastructure.Documents;
using Microsoft.Azure.Cosmos;
using IPLMerchandise.Application.Exceptions;

namespace IPLMerchandise.Infrastructure.Repositories;

public sealed class CosmosOrderRepository : IOrderRepository
{
    private readonly Container _container;

    public CosmosOrderRepository(CosmosClient client)
    {
        _container = client.GetContainer(
            "IplMerchandiseDb",
            "Orders");
    }

    public async Task<Order?> GetByIdAsync(
        string customerId,
        string orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<OrderDocument>(
                orderId,
                new PartitionKey(customerId),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(
        string customerId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            """
            SELECT * FROM c
            WHERE c.documentType = @documentType
            AND c.customerId = @customerId
            AND c.idempotencyKey = @idempotencyKey
            """)
            .WithParameter("@documentType", "Order")
            .WithParameter("@customerId", customerId)
            .WithParameter("@idempotencyKey", idempotencyKey);

        var options = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(customerId),
            MaxItemCount = 1
        };

        using var iterator =
            _container.GetItemQueryIterator<OrderDocument>(
                query,
                requestOptions: options);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(
                cancellationToken);

            var document = response.FirstOrDefault();

            if (document is not null)
                return MapToDomain(document);
        }

        return null;
    }

    public async Task CreateAsync(
    Order order,
    CancellationToken cancellationToken)
    {
        var document = MapToDocument(order);

        try
        {
            await _container.CreateItemAsync(
                document,
                new PartitionKey(order.CustomerId),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException ex)
            when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new DuplicateCheckoutException(
                order.CustomerId,
                order.IdempotencyKey ?? string.Empty,
                ex);
        }
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            """
            SELECT * FROM c
            WHERE c.documentType = @documentType
            AND c.customerId = @customerId
            ORDER BY c.createdAtUtc DESC
            """)
            .WithParameter("@documentType", "Order")
            .WithParameter("@customerId", customerId);

        var options = new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(customerId)
        };

        var orders = new List<Order>();

        using var iterator =
            _container.GetItemQueryIterator<OrderDocument>(
                query,
                requestOptions: options);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(
                cancellationToken);

            orders.AddRange(response.Select(MapToDomain));
        }

        return orders;
    }

    public async Task UpdateAsync(
    Order order,
    CancellationToken cancellationToken)
    {
        var document = MapToDocument(order);

        await _container.ReplaceItemAsync(
            document,
            order.Id,
            new PartitionKey(order.CustomerId),
            cancellationToken: cancellationToken);
    }

    private static Order MapToDomain(OrderDocument document)
    {
        return new Order
        {
            Id = document.Id,
            CustomerId = document.CustomerId,
            OrderNumber = document.OrderNumber,

            Items = document.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                FranchiseId = item.FranchiseId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList(),

            Subtotal = document.Subtotal,
            ShippingCharges = document.ShippingCharges,
            TotalAmount = document.TotalAmount,
            Currency = document.Currency,

            Status = Enum.Parse<OrderStatus>(
                document.Status, ignoreCase: true),

            PaymentStatus = Enum.Parse<PaymentStatus>(
                document.PaymentStatus, ignoreCase: true),

            ShippingAddress = new ShippingAddress
            {
                FullName = document.ShippingAddress.FullName,
                PhoneNumber = document.ShippingAddress.PhoneNumber,
                AddressLine1 = document.ShippingAddress.AddressLine1,
                AddressLine2 = document.ShippingAddress.AddressLine2,
                City = document.ShippingAddress.City,
                State = document.ShippingAddress.State,
                PostalCode = document.ShippingAddress.PostalCode,
                Country = document.ShippingAddress.Country
            },

            IdempotencyKey = document.IdempotencyKey,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc
        };
    }

    private static OrderDocument MapToDocument(Order order)
    {
        return new OrderDocument
        {
            Id = order.Id,
            DocumentType = "Order",
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,

            Items = order.Items.Select(item => new OrderItemDocument
            {
                ProductId = item.ProductId,
                FranchiseId = item.FranchiseId,
                ProductName = item.ProductName,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity
            }).ToList(),

            Subtotal = order.Subtotal,
            ShippingCharges = order.ShippingCharges,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,

            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),

            ShippingAddress = new ShippingAddressDocument
            {
                FullName = order.ShippingAddress.FullName,
                PhoneNumber = order.ShippingAddress.PhoneNumber,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },

            IdempotencyKey = order.IdempotencyKey,
            CreatedAtUtc = order.CreatedAtUtc,
            UpdatedAtUtc = order.UpdatedAtUtc
        };
    }
}
