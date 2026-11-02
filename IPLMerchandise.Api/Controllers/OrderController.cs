using IPLMerchandise.Application.DTOs.Orders;
using IPLMerchandise.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPLMerchandise.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId}/orders")]
public sealed class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST: api/customers/{customerId}/orders/checkout
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout(
        string customerId,
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.CheckoutAsync(
            customerId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetOrder),
            new
            {
                customerId,
                orderId = order.Id
            },
            order);
    }

    // GET: api/customers/{customerId}/orders/{orderId}
    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        string customerId,
        string orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderAsync(
            customerId,
            orderId,
            cancellationToken);

        return Ok(order);
    }

    // GET: api/customers/{customerId}/orders
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(
        string customerId,
        CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetCustomerOrdersAsync(
            customerId,
            cancellationToken);

        return Ok(orders);
    }

    // POST: api/customers/{customerId}/orders/{orderId}/payment
    [HttpPost("{orderId}/payment")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProcessPayment(
        string customerId,
        string orderId,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.ProcessPaymentAsync(
            customerId,
            orderId,
            request,
            cancellationToken);

        return Ok(order);
    }
}
