
using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Domain.Entities;

public sealed class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CustomerId { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ShippingCharges { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public ShippingAddress ShippingAddress { get; set; } = new();

    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class OrderItem
{
    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed class ShippingAddress
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = "India";
}
