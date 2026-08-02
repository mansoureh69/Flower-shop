using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Orders.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

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
        var customerId = currentUserService.UserId;
        if (customerId is null)
            return Result<OrderResponse>.Failure("An authenticated customer is required.");

        var order = new Order(customerId.Value, request.Notes);

        foreach (var item in request.Items)
        {
            // Load the Product from the repository to validate availability
            // and to snapshot its authoritative Name and Price (Money)
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null || product.IsDeleted)
                return Result<OrderResponse>.Failure($"Product not found: {item.ProductId}");

            if (!product.IsAvailable)
                return Result<OrderResponse>.Failure($"Product is not available: {product.Name}");

            // Pass snapshot values to Order.AddItem
            // The handler loads Product but passes only its data, not the entity itself
            order.AddItem(
                productId: product.Id,
                productName: product.Name,
                unitPrice: product.Price,
                quantity: item.Quantity,
                notes: item.Notes);
        }

        order.EnsureReadyForPayment();

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
