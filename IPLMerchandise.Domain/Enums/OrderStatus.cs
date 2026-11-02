namespace IPLMerchandise.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    InventoryReserved = 2,
    PaymentPending = 3,
    Confirmed = 4,
    Processing = 5,
    Shipped = 6,
    Delivered = 7,
    Cancelled = 8,
    Failed = 9
}