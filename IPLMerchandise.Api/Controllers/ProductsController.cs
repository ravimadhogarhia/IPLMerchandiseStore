using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/products")]
[EnableRateLimiting("ApiPolicy")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? franchiseId,
        [FromQuery] ProductCategory? category,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.SearchAsync(
            search,
            franchiseId,
            category,
            pageSize,
            continuationToken,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{franchiseId}/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            franchiseId,
            productId,
            cancellationToken);

        if (product is null)
            return NotFound(new { message = "Product not found." });

        return Ok(product);
    }
}