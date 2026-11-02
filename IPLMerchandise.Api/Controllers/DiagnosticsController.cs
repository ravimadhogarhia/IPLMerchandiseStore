using IPLMerchandise.Api.Diagnostics;
using IPLMerchandise.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly IWebHostEnvironment _environment;

    public DiagnosticsController(
        ICartRepository cartRepository,
        IWebHostEnvironment environment)
    {
        _cartRepository = cartRepository;
        _environment = environment;
    }

    [HttpPost("cart-concurrency")]
    public async Task<IActionResult> TestCartConcurrency(
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        await CartConcurrencyTest.RunAsync(
            _cartRepository,
            cancellationToken);

        return Ok(new
        {
            Message = "Cart concurrency test completed successfully."
        });
    }
}