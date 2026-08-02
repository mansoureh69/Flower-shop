using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Orders.Common;

namespace SweetFlowerShop.Application.Features.Orders.PlaceOrder;

public record PlaceOrderCommand(
    string? Notes,
    List<OrderItemRequest> Items) : IRequest<Result<OrderResponse>>;

public record OrderItemRequest(
    Guid ProductId,
    int Quantity,
    string? Notes);
