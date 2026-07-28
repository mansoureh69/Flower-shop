using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetFlowerShop.Application.Features.Orders.PlaceOrder;

namespace Flower_shop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Place a new order. Requires authentication.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PlaceOrder(PlaceOrderCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(null, new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { errors = result.Errors });
    }
}
