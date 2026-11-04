
using IPLMerchandise.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[EnableRateLimiting("ApiPolicy")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{franchiseId}/{productId}")]
    public async Task<IActionResult> GetInventory(
        string franchiseId,
        string productId,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetInventoryAsync(
            franchiseId,
            productId,
            cancellationToken);

        return Ok(result);
    }
}
