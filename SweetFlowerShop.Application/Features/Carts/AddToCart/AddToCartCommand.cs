using MediatR;
using SweetFlowerShop.Application.Common;
using SweetFlowerShop.Application.Features.Carts.Common;

namespace SweetFlowerShop.Application.Features.Carts.AddToCart;

public record AddToCartCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity) : IRequest<Result<CartResponse>>;
