using SweetFlowerShop.Domain.Enums;

namespace SweetFlowerShop.Application.Features.Orders.Common;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    string? Notes,
    DeliveryInfoResponse Delivery,
    IReadOnlyList<OrderItemResponse> Items);

public record DeliveryInfoResponse(
    string RecipientName,
    string RecipientPhone,
    string Street,
    string City,
    string ZipCode,
    DateTime? ScheduledDate,
    string? GiftMessage);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal TotalPrice);
