namespace IPLMerchandise.Application.DTOs.Orders;

public sealed class CheckoutRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;

    public ShippingAddressDto ShippingAddress { get; set; } = new();
}