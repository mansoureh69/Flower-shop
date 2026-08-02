using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Orders.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;
using SweetFlowerShop.Domain.ValueObjects;

namespace SweetFlowerShop.Application.Features.Orders.PlaceOrder;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PlaceOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = currentUserService.CustomerId;
        if (customerId is null)
            return Result<OrderResponse>.Failure("An authenticated customer is required.");

        var snapshots = new List<OrderLineSnapshot>(request.Items.Count);

        foreach (var item in request.Items)
        {
            // Load the Product from the repository to validate availability
            // and to snapshot its authoritative Name and Price (Money)
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null || product.IsDeleted)
                return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");

            if (!product.IsAvailable)
                return Result<OrderResponse>.Failure($"Product is not available: {product.Name}");

            snapshots.Add(new OrderLineSnapshot(
                product.Id,
                product.Name,
                product.Price,
                item.Quantity,
                item.Notes));
        }

        var deliveryInfo = new DeliveryInfo(
            request.Delivery.RecipientName,
            request.Delivery.RecipientPhone,
            request.Delivery.Street,
            request.Delivery.City,
            request.Delivery.ZipCode,
            request.Delivery.ScheduledDate,
            request.Delivery.GiftMessage);

        var order = Order.Place(customerId.Value, deliveryInfo, snapshots, request.Notes);

        // Persist the order with all items snapshotted
        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<OrderResponse>.Success(order.ToResponse());
    }
}

internal static class OrderMappingExtensions
{
    public static OrderResponse ToResponse(this Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.Notes,
            new DeliveryInfoResponse(
                order.DeliveryInfo.RecipientName,
                order.DeliveryInfo.RecipientPhone,
                order.DeliveryInfo.Street,
                order.DeliveryInfo.City,
                order.DeliveryInfo.ZipCode,
                order.DeliveryInfo.ScheduledDate,
                order.DeliveryInfo.GiftMessage),
            order.Items
                .Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice.Amount,
                    i.UnitPrice.Currency,
                    i.Quantity,
                    i.TotalPrice))
                .ToList());
}
