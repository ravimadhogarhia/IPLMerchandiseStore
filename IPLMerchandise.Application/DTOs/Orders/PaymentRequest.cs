namespace IPLMerchandise.Application.DTOs.Orders;

public sealed class PaymentRequest
{
    public bool IsSuccessful { get; set; }

    public string PaymentReference { get; set; } = string.Empty;
}