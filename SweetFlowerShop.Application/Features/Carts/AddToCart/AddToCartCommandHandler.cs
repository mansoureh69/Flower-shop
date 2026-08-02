using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Carts.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Features.Carts.AddToCart;

public sealed class AddToCartCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddToCartCommand, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var customerId = currentUserService.CustomerId;
        if (customerId is null)
            return Result<CartResponse>.Failure("An authenticated customer is required.");

        // Step 1: Validate related entity (Product)
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.IsDeleted)
            return Result<CartResponse>.Failure($"Product not found: {request.ProductId}");

        if (!product.IsAvailable)
            return Result<CartResponse>.Failure($"Product is not available: {product.Name}");

        // Step 2: Load or create cart
        var cart = await cartRepository.GetByCustomerIdAsync(customerId.Value, cancellationToken);

        if (cart is null)
        {
            cart = new Cart(customerId.Value);
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        // Step 3: Add item to cart with validated product
        cart.AddItem(product.Id, product.Name, product.Price, request.Quantity);

        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}

internal static class CartMappingExtensions
{
    public static CartResponse ToResponse(this Cart cart) =>
        new(
            cart.Id,
            cart.CustomerId,
            cart.Items.Select(i =>
                new CartItemResponse(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.SnapshotPrice.Amount,
                    i.Quantity)).ToList());
}
