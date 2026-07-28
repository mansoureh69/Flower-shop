using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Carts.Common;
using SweetFlowerShop.Application.Interfaces;
using SweetFlowerShop.Domain.Entities;

namespace SweetFlowerShop.Application.Features.Carts.AddToCart;

public sealed class AddToCartCommandHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddToCartCommand, Result<CartResponse>>
{
    public async Task<Result<CartResponse>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart(request.CustomerId);
            await cartRepository.AddAsync(cart, cancellationToken);
        }

        cart.AddItem(request.ProductId, request.Quantity);

        cartRepository.Update(cart);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CartResponse>.Success(cart.ToResponse());
    }
}

internal static class CartMappingExtensions
{
    public static CartResponse ToResponse(this Cart cart) => new(
        cart.Id,
        cart.CustomerId,
        cart.Items.Select(i => new CartItemResponse(i.Id, i.ProductId, i.Quantity)).ToList());
}
