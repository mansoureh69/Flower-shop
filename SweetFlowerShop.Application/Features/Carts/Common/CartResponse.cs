namespace SweetFlowerShop.Application.Features.Carts.Common;

public record CartResponse(
    Guid Id,
    Guid CustomerId,
    IReadOnlyList<CartItemResponse> Items);

public record CartItemResponse(
    Guid Id,
    Guid ProductId,
    int Quantity);
