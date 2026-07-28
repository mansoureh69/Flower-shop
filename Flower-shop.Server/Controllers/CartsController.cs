using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetFlowerShop.Application.Features.Carts.AddToCart;

namespace Flower_shop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CartsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Add an item to the customer's cart. Creates cart if it doesn't exist.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddItem(AddToCartCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { errors = result.Errors });
    }
}
