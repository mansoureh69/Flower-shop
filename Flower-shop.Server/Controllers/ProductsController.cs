using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SweetFlowerShop.Application.Features.Products.CreateProduct;
using SweetFlowerShop.Application.Features.Products.GetProducts;

namespace Flower_shop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get all products. Optionally filter by category or availability.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? availableOnly,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsQuery(categoryId, availableOnly), ct);
        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new product. Requires Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(null, result.Value)
            : BadRequest(new { errors = result.Errors });
    }
}
