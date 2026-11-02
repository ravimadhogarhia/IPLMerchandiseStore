
using IPLMerchandise.Application.DTOs.Carts;
using IPLMerchandise.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId}/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // GET: api/customers/CUST001/cart
    [HttpGet]
    public async Task<IActionResult> GetCart(
        string customerId,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.GetCartAsync(
            customerId,
            cancellationToken);

        return Ok(result);
    }

    // POST: api/customers/CUST001/cart/items
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        string customerId,
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.AddItemAsync(
            customerId,
            request,
            cancellationToken);

        return Ok(result);
    }

    // PUT: api/customers/CUST001/cart/items/MI/MI-JERSEY-001
    [HttpPut("items/{franchiseId}/{productId}")]
    public async Task<IActionResult> UpdateItem(
        string customerId,
        string franchiseId,
        string productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateItemAsync(
            customerId,
            franchiseId,
            productId,
            request,
            cancellationToken);

        return Ok(result);
    }

    // DELETE: api/customers/CUST001/cart/items/MI/MI-JERSEY-001
    [HttpDelete("items/{franchiseId}/{productId}")]
    public async Task<IActionResult> RemoveItem(
        string customerId,
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        await _cartService.RemoveItemAsync(
            customerId,
            franchiseId,
            productId,
            cancellationToken);

        return NoContent();
    }
}
