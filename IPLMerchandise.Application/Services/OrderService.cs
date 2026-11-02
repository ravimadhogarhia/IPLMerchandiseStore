using IPLMerchandise.Application.DTOs.Orders;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
namespace IPLMerchandise.Application.Services;
using IPLMerchandise.Application.Exceptions;
using System.ComponentModel.Design;

public sealed class OrderService : IOrderService
{
    private const decimal ShippingCharge = 99m;

    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryReservationService _reservationService;
    private readonly IPaymentService _paymentService;

    public OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IInventoryReservationService reservationService,
    IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _reservationService = reservationService;
        _paymentService = paymentService;
    }

    public async Task<OrderDto> CheckoutAsync(
        string customerId,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCheckoutRequest(customerId, request);

        // Step 1: Check for an existing order using the idempotency key.
        var existingOrder =
            await _orderRepository.GetByIdempotencyKeyAsync(
                customerId,
                request.IdempotencyKey.Trim(),
                cancellationToken);

        if (existingOrder is not null)
            return MapToDto(existingOrder);

        // Step 2: Retrieve customer cart.
        var cart = await _cartRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        if (cart is null || cart.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Your shopping cart is empty.");
        }

        // Step 3: Validate products and prepare order items.
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cart.Items)
        {
            var product = await _productRepository.GetByIdAsync(
                cartItem.FranchiseId,
                cartItem.ProductId,
                cancellationToken);

            if (product is null || !product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product {cartItem.ProductId} is unavailable.");
            }

            if (product.Price <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid price for product {product.Name}.");
            }

            if (cartItem.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid quantity for product {product.Name}.");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                FranchiseId = product.FranchiseId,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity
            });
        }

        // Step 4: Calculate order amount.
        var subtotal = orderItems.Sum(item => item.LineTotal);
        var shippingCharges = ShippingCharge;
        var totalAmount = subtotal + shippingCharges;

        var now = DateTime.UtcNow;

        // Step 5: Prepare order.
        var order = new Order
        {
            Id = GenerateDeterministicOrderId(customerId,request.IdempotencyKey),
            CustomerId = customerId,
            OrderNumber = $"IPL-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..21],
            Items = orderItems,
            Subtotal = subtotal,
            ShippingCharges = shippingCharges,
            TotalAmount = totalAmount,
            Currency = cart.Currency,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            IdempotencyKey = request.IdempotencyKey.Trim(),
            ShippingAddress = MapAddress(request.ShippingAddress),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        // Step 6: Reserve inventory and create order.
        var reservedItems = new List<OrderItem>();

        try
        {
            foreach (var item in order.Items)
            {
                await _reservationService.ReserveAsync(
                    item.FranchiseId,
                    item.ProductId,
                    item.Quantity,
                    cancellationToken);

                reservedItems.Add(item);
            }

            // Create order only after all inventory reservations succeed.
            try
            {
                await _orderRepository.CreateAsync(
                    order,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // If the exception is due to a conflict (duplicate order), handle accordingly.
                // You may need to check ex.Message or ex.HResult for conflict detection,
                // or add a reference to Microsoft.Azure.Cosmos if CosmosException is required.
                // For now, rethrow as DuplicateCheckoutException only if conflict is detected.
                // Otherwise, rethrow the original exception.

                // Example: If you want to check for conflict by status code (if available)
                // if (ex is SomeConflictExceptionType) { ... }

                throw new DuplicateCheckoutException(
                    customerId,
                    request.IdempotencyKey,
                    ex);
            }
        }
        catch (Exception checkoutException)
        {
            // Compensating transaction: release previously reserved stock.
            var releaseErrors = new List<Exception>();

            foreach (var item in reservedItems.AsEnumerable().Reverse())
            {
                try
                {
                    await _reservationService.ReleaseAsync(
                        item.FranchiseId,
                        item.ProductId,
                        item.Quantity,
                        CancellationToken.None);
                }
                catch (Exception releaseException)
                {
                    releaseErrors.Add(releaseException);
                }
            }

            if (releaseErrors.Count > 0)
            {
                throw new AggregateException(
                    "Checkout failed and inventory compensation was incomplete. " +
                    "Manual inventory reconciliation is required.",
                    new[] { checkoutException }.Concat(releaseErrors));
            }

            if (checkoutException is DuplicateCheckoutException)
            {
                var existingOrderToDiscard =
                    await _orderRepository.GetByIdempotencyKeyAsync(
                        customerId,
                        request.IdempotencyKey.Trim(),
                        cancellationToken);

                if (existingOrderToDiscard is not null)
                    return MapToDto(existingOrderToDiscard);
            }

            throw;
        }

        // Step 7: Clear cart after successful order creation.
        cart.Items.Clear();
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _cartRepository.UpsertAsync(
            cart,
            cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderDto> GetOrderAsync(
        string customerId,
        string orderId,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);

        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID is required.");

        var order = await _orderRepository.GetByIdAsync(
            customerId,
            orderId,
            cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        return MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetCustomerOrdersAsync(
        string customerId,
        CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);

        var orders = await _orderRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> ProcessPaymentAsync(
    string customerId,
    string orderId,
    PaymentRequest request,
    CancellationToken cancellationToken)
    {
        ValidateCustomerId(customerId);

        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID is required.");

        if (request is null)
            throw new ArgumentException("Payment request is required.");

        if (string.IsNullOrWhiteSpace(request.PaymentReference))
            throw new ArgumentException("Payment reference is required.");

        var order = await _orderRepository.GetByIdAsync(
            customerId,
            orderId,
            cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            return MapToDto(order);

        if (order.Status == OrderStatus.Cancelled ||
            order.Status == OrderStatus.Failed)
        {
            throw new InvalidOperationException(
                "Payment cannot be processed for this order.");
        }

        if (order.PaymentStatus != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "Payment is not in a processable state.");
        }

        order.PaymentStatus = PaymentStatus.Processing;
        order.Status = OrderStatus.PaymentPending;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);

        try
        {
            var paymentSucceeded =
                request.IsSuccessful &&
                await _paymentService.ProcessPaymentAsync(
                    order.TotalAmount,
                    order.Currency,
                    request.PaymentReference,
                    cancellationToken);

            if (paymentSucceeded)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Confirmed;
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.Status = OrderStatus.Failed;

                await ReleaseOrderInventoryAsync(
                    order,
                    cancellationToken);
            }
        }
        catch
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.Status = OrderStatus.Failed;

            await ReleaseOrderInventoryAsync(
                order,
                CancellationToken.None);

            throw;
        }

        order.UpdatedAtUtc = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(
            order,
            cancellationToken);

        return MapToDto(order);
    }

    private async Task ReleaseOrderInventoryAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            await _reservationService.ReleaseAsync(
                item.FranchiseId,
                item.ProductId,
                item.Quantity,
                cancellationToken);
        }
    }

    private static void ValidateCheckoutRequest(
        string customerId,
        CheckoutRequest request)
    {
        ValidateCustomerId(customerId);

        if (request is null)
            throw new ArgumentException("Checkout request is required.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            throw new ArgumentException(
                "Idempotency key is required.");
        }

        if (request.IdempotencyKey.Trim().Length > 128)
        {
            throw new ArgumentException(
                "Idempotency key cannot exceed 128 characters.");
        }

        var address = request.ShippingAddress;

        if (address is null ||
            string.IsNullOrWhiteSpace(address.FullName) ||
            string.IsNullOrWhiteSpace(address.PhoneNumber) ||
            string.IsNullOrWhiteSpace(address.AddressLine1) ||
            string.IsNullOrWhiteSpace(address.City) ||
            string.IsNullOrWhiteSpace(address.State) ||
            string.IsNullOrWhiteSpace(address.PostalCode) ||
            string.IsNullOrWhiteSpace(address.Country))
        {
            throw new ArgumentException(
                "Complete shipping address details are required.");
        }
    }

    private static void ValidateCustomerId(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Customer ID is required.");
        }
    }

    private static ShippingAddress MapAddress(
        ShippingAddressDto address)
    {
        return new ShippingAddress
        {
            FullName = address.FullName.Trim(),
            PhoneNumber = address.PhoneNumber.Trim(),
            AddressLine1 = address.AddressLine1.Trim(),
            AddressLine2 = address.AddressLine2?.Trim(),
            City = address.City.Trim(),
            State = address.State.Trim(),
            PostalCode = address.PostalCode.Trim(),
            Country = address.Country.Trim()
        };
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,

            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                FranchiseId = item.FranchiseId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.LineTotal
            }).ToList(),

            Subtotal = order.Subtotal,
            ShippingCharges = order.ShippingCharges,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,

            ShippingAddress = new ShippingAddressDto
            {
                FullName = order.ShippingAddress.FullName,
                PhoneNumber = order.ShippingAddress.PhoneNumber,
                AddressLine1 = order.ShippingAddress.AddressLine1,
                AddressLine2 = order.ShippingAddress.AddressLine2,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                PostalCode = order.ShippingAddress.PostalCode,
                Country = order.ShippingAddress.Country
            },

            CreatedAtUtc = order.CreatedAtUtc
        };
    }

    private static string GenerateDeterministicOrderId(
    string customerId,
    string idempotencyKey)
    {
        var input = $"{customerId}:{idempotencyKey.Trim()}";

        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
