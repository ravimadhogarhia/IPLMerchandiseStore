
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPLMerchandise.Application.DTOs.Carts;
using IPLMerchandise.Application.DTOs.Orders;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;
using Moq;
using Xunit;

namespace IPLMerchandise.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IInventoryReservationService> _reservationServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;

    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _reservationServiceMock = new Mock<IInventoryReservationService>();
        _paymentServiceMock = new Mock<IPaymentService>();

        _service = new OrderService(
            _orderRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _reservationServiceMock.Object,
            _paymentServiceMock.Object);
    }

    private static ShippingAddressDto CreateAddress()
    {
        return new ShippingAddressDto
        {
            FullName = "Test Customer",
            PhoneNumber = "9876543210",
            AddressLine1 = "Flat 101, Test Apartments",
            AddressLine2 = "Main Road",
            City = "Hyderabad",
            State = "Telangana",
            PostalCode = "500001",
            Country = "India"
        };
    }

    private static CheckoutRequest CreateCheckoutRequest()
    {
        return new CheckoutRequest
        {
            IdempotencyKey = "CHECKOUT-KEY-001",
            ShippingAddress = CreateAddress()
        };
    }

    private static Product CreateProduct()
    {
        return new Product
        {
            Id = "MI-JERSEY-001",
            Name = "Mumbai Indians Jersey",
            Description = "MI Jersey",
            FranchiseId = "MI",
            FranchiseName = "Mumbai Indians",
            Price = 1999m,
            Currency = "INR",
            ImageUrl = "https://example.com/mi-jersey.jpg",
            IsActive = true
        };
    }

    private static Cart CreateCart()
    {
        return new Cart
        {
            Id = "CUST-001",
            CustomerId = "CUST-001",
            Currency = "INR",
            Items = new List<CartItem>
            {
                new()
                {
                    ProductId = "MI-JERSEY-001",
                    FranchiseId = "MI",
                    ProductName = "Mumbai Indians Jersey",
                    UnitPrice = 1999m,
                    Quantity = 2
                }
            },
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static Order CreateOrder(
        PaymentStatus paymentStatus = PaymentStatus.Pending,
        OrderStatus status = OrderStatus.Pending)
    {
        return new Order
        {
            Id = "ORDER-001",
            CustomerId = "CUST-001",
            OrderNumber = "IPL-20261102-123456",
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = "MI-JERSEY-001",
                    FranchiseId = "MI",
                    ProductName = "Mumbai Indians Jersey",
                    UnitPrice = 1999m,
                    Quantity = 2
                }
            },
            Subtotal = 3998m,
            ShippingCharges = 99m,
            TotalAmount = 4097m,
            Currency = "INR",
            Status = status,
            PaymentStatus = paymentStatus,
            IdempotencyKey = "CHECKOUT-KEY-001",
            ShippingAddress = new ShippingAddress
            {
                FullName = "Test Customer",
                PhoneNumber = "9876543210",
                AddressLine1 = "Flat 101, Test Apartments",
                City = "Hyderabad",
                State = "Telangana",
                PostalCode = "500001",
                Country = "India"
            },
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private void SetupSuccessfulCheckout(Cart? cart = null)
    {
        _orderRepositoryMock
            .Setup(x => x.GetByIdempotencyKeyAsync(
                "CUST-001",
                "CHECKOUT-KEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart ?? CreateCart());

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProduct());

        _reservationServiceMock
            .Setup(x => x.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                2,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderRepositoryMock
            .Setup(x => x.CreateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cartRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // CHECKOUT TESTS

    [Fact]
    public async Task CheckoutAsync_ShouldCreateOrder_WhenCheckoutIsValid()
    {
        // Arrange
        var cart = CreateCart();
        SetupSuccessfulCheckout(cart);

        // Act
        var result = await _service.CheckoutAsync(
            "CUST-001",
            CreateCheckoutRequest(),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CUST-001", result.CustomerId);
        Assert.Equal(3998m, result.Subtotal);
        Assert.Equal(99m, result.ShippingCharges);
        Assert.Equal(4097m, result.TotalAmount);
        Assert.Equal("INR", result.Currency);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(PaymentStatus.Pending, result.PaymentStatus);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);

        _orderRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _reservationServiceMock.Verify(
            x => x.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task CheckoutAsync_ShouldReturnExistingOrder_WhenIdempotencyKeyExists()
    {
        // Arrange
        var existingOrder = CreateOrder();

        _orderRepositoryMock
            .Setup(x => x.GetByIdempotencyKeyAsync(
                "CUST-001",
                "CHECKOUT-KEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        // Act
        var result = await _service.CheckoutAsync(
            "CUST-001",
            CreateCheckoutRequest(),
            CancellationToken.None);

        // Assert
        Assert.Equal(existingOrder.Id, result.Id);
        Assert.Equal(existingOrder.TotalAmount, result.TotalAmount);

        _cartRepositoryMock.Verify(
            x => x.GetByCustomerIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orderRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ShouldThrowInvalidOperationException_WhenCartIsEmpty()
    {
        // Arrange
        SetupSuccessfulCheckout(
            new Cart
            {
                Id = "CUST-001",
                CustomerId = "CUST-001",
                Currency = "INR",
                Items = new List<CartItem>()
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CheckoutAsync(
                "CUST-001",
                CreateCheckoutRequest(),
                CancellationToken.None));
    }

    [Fact]
    public async Task CheckoutAsync_ShouldThrowInvalidOperationException_WhenProductIsUnavailable()
    {
        // Arrange
        SetupSuccessfulCheckout();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CheckoutAsync(
                "CUST-001",
                CreateCheckoutRequest(),
                CancellationToken.None));

        _reservationServiceMock.Verify(
            x => x.ReserveAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ShouldThrowInvalidOperationException_WhenInventoryReservationFails()
    {
        // Arrange
        SetupSuccessfulCheckout();

        _reservationServiceMock
            .Setup(x => x.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                2,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(
                "Insufficient stock."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CheckoutAsync(
                "CUST-001",
                CreateCheckoutRequest(),
                CancellationToken.None));

        _orderRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _reservationServiceMock.Verify(
            x => x.ReleaseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_ShouldThrowArgumentException_WhenIdempotencyKeyIsMissing()
    {
        // Arrange
        var request = CreateCheckoutRequest();
        request.IdempotencyKey = string.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CheckoutAsync(
                "CUST-001",
                request,
                CancellationToken.None));
    }

    // ORDER RETRIEVAL TESTS

    [Fact]
    public async Task GetOrderAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var order = CreateOrder();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _service.GetOrderAsync(
            "CUST-001",
            "ORDER-001",
            CancellationToken.None);

        // Assert
        Assert.Equal("ORDER-001", result.Id);
        Assert.Equal("CUST-001", result.CustomerId);
        Assert.Equal(4097m, result.TotalAmount);
    }

    [Fact]
    public async Task GetOrderAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-999",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetOrderAsync(
                "CUST-001",
                "ORDER-999",
                CancellationToken.None));
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_ShouldReturnCustomerOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateOrder(),
            CreateOrder()
        };

        orders[1].Id = "ORDER-002";

        _orderRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _service.GetCustomerOrdersAsync(
            "CUST-001",
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("ORDER-001", result[0].Id);
        Assert.Equal("ORDER-002", result[1].Id);
    }

    // PAYMENT TESTS

    [Fact]
    public async Task ProcessPaymentAsync_ShouldConfirmOrder_WhenPaymentSucceeds()
    {
        // Arrange
        var order = CreateOrder();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _paymentServiceMock
            .Setup(x => x.ProcessPaymentAsync(
                4097m,
                "INR",
                "PAY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new PaymentRequest
        {
            IsSuccessful = true,
            PaymentReference = "PAY-001"
        };

        // Act
        var result = await _service.ProcessPaymentAsync(
            "CUST-001",
            "ORDER-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Paid, result.PaymentStatus);
        Assert.Equal(OrderStatus.Confirmed, result.Status);

        _paymentServiceMock.Verify(
            x => x.ProcessPaymentAsync(
                4097m,
                "INR",
                "PAY-001",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _orderRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _reservationServiceMock.Verify(
            x => x.ReleaseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldFailOrderAndReleaseInventory_WhenPaymentFails()
    {
        // Arrange
        var order = CreateOrder();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _reservationServiceMock
            .Setup(x => x.ReleaseAsync(
                "MI",
                "MI-JERSEY-001",
                2,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new PaymentRequest
        {
            IsSuccessful = false,
            PaymentReference = "PAY-FAILED-001"
        };

        // Act
        var result = await _service.ProcessPaymentAsync(
            "CUST-001",
            "ORDER-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Failed, result.PaymentStatus);
        Assert.Equal(OrderStatus.Failed, result.Status);

        _paymentServiceMock.Verify(
            x => x.ProcessPaymentAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _reservationServiceMock.Verify(
            x => x.ReleaseAsync(
                "MI",
                "MI-JERSEY-001",
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldReturnExistingOrder_WhenAlreadyPaid()
    {
        // Arrange
        var order = CreateOrder(
            PaymentStatus.Paid,
            OrderStatus.Confirmed);

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var request = new PaymentRequest
        {
            IsSuccessful = true,
            PaymentReference = "PAY-001"
        };

        // Act
        var result = await _service.ProcessPaymentAsync(
            "CUST-001",
            "ORDER-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Paid, result.PaymentStatus);

        _paymentServiceMock.Verify(
            x => x.ProcessPaymentAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _orderRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "CUST-001",
                "ORDER-999",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var request = new PaymentRequest
        {
            IsSuccessful = true,
            PaymentReference = "PAY-001"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ProcessPaymentAsync(
                "CUST-001",
                "ORDER-999",
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentException_WhenPaymentReferenceIsMissing()
    {
        // Arrange
        var request = new PaymentRequest
        {
            IsSuccessful = true,
            PaymentReference = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ProcessPaymentAsync(
                "CUST-001",
                "ORDER-001",
                request,
                CancellationToken.None));
    }
}
