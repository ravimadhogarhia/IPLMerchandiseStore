
using IPLMerchandise.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/inventory")]
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
