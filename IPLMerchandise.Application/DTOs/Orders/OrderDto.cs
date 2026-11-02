using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Application.DTOs.Orders;

public sealed class OrderDto
{
    public string Id { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public List<OrderItemDto> Items { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ShippingCharges { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public OrderStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public ShippingAddressDto ShippingAddress { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; }
}

public sealed class OrderItemDto
{
    public string ProductId { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}