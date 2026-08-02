namespace SweetFlowerShop.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,
    Confirmed,
    Preparing,
    ReadyForDelivery,
    OutForDelivery,
    Delivered,
    Cancelled
}
