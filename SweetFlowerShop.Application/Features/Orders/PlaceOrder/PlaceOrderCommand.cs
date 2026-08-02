using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Orders.Common;

namespace SweetFlowerShop.Application.Features.Orders.PlaceOrder;

public record PlaceOrderCommand(
    DeliveryInfoRequest Delivery,
    string? Notes,
    List<OrderItemRequest> Items) : IRequest<Result<OrderResponse>>;

public record DeliveryInfoRequest(
    string RecipientName,
    string RecipientPhone,
    string Street,
    string City,
    string ZipCode,
    DateTime? ScheduledDate,
    string? GiftMessage);

public record OrderItemRequest(
    Guid ProductId,
    int Quantity,
    string? Notes);
