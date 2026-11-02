
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPLMerchandise.Application.DTOs.Carts;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Domain.Entities;
using Moq;
using Xunit;

namespace IPLMerchandise.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly CartService _service;

    public CartServiceTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();

        _service = new CartService(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _inventoryRepositoryMock.Object);
    }

    private static Product CreateProduct(
        bool isActive = true,
        decimal price = 1999m,
        string currency = "INR")
    {
        return new Product
        {
            Id = "MI-JERSEY-001",
            Name = "Mumbai Indians Jersey",
            Description = "MI Jersey",
            FranchiseId = "MI",
            FranchiseName = "Mumbai Indians",
            Price = price,
            Currency = currency,
            ImageUrl = "https://example.com/mi-jersey.jpg",
            IsActive = isActive
        };
    }

    private static Inventory CreateInventory(
        int available = 100,
        int reserved = 0)
    {
        return new Inventory
        {
            Id = "MI-MI-JERSEY-001",
            ProductId = "MI-JERSEY-001",
            FranchiseId = "MI",
            AvailableQuantity = available,
            ReservedQuantity = reserved
        };
    }

    private static Cart CreateCart(
        string customerId = "CUST-001",
        int quantity = 2)
    {
        return new Cart
        {
            Id = customerId,
            CustomerId = customerId,
            Currency = "INR",
            Items = new List<CartItem>
            {
                new()
                {
                    FranchiseId = "MI",
                    ProductId = "MI-JERSEY-001",
                    ProductName = "Mumbai Indians Jersey",
                    UnitPrice = 1999m,
                    Quantity = quantity
                }
            },
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private void SetupProductAndInventory(
        Product? product = null,
        Inventory? inventory = null)
    {
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product ?? CreateProduct());

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory ?? CreateInventory());
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnExistingCart()
    {
        // Arrange
        var cart = CreateCart();

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var result = await _service.GetCartAsync(
            "CUST-001",
            CancellationToken.None);

        // Assert
        Assert.Equal("CUST-001", result.CustomerId);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(3998m, result.Subtotal);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        // Arrange
        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        var result = await _service.GetCartAsync(
            "CUST-001",
            CancellationToken.None);

        // Assert
        Assert.Equal("CUST-001", result.CustomerId);
        Assert.Empty(result.Items);
        Assert.Equal(0m, result.Subtotal);
        Assert.Equal("INR", result.Currency);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddProduct_WhenStockIsAvailable()
    {
        // Arrange
        SetupProductAndInventory();

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        Cart? savedCart = null;

        _cartRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>(
                (cart, _) => savedCart = cart)
            .Returns(Task.CompletedTask);

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 2
        };

        // Act
        var result = await _service.AddItemAsync(
            "CUST-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(1999m, result.Items[0].UnitPrice);
        Assert.Equal(3998m, result.Subtotal);

        Assert.NotNull(savedCart);
        Assert.Single(savedCart.Items);

        _cartRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        SetupProductAndInventory();

        var cart = CreateCart(quantity: 2);

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 3
        };

        // Act
        var result = await _service.AddItemAsync(
            "CUST-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].Quantity);
        Assert.Equal(9995m, result.Subtotal);
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddItemAsync(
                "CUST-001",
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenProductIsInactive()
    {
        // Arrange
        SetupProductAndInventory(
            product: CreateProduct(isActive: false));

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddItemAsync(
                "CUST-001",
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        // Arrange
        SetupProductAndInventory(
            inventory: CreateInventory(available: 5, reserved: 2));

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 4
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddItemAsync(
                "CUST-001",
                request,
                CancellationToken.None));

        _cartRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowArgumentException_WhenQuantityIsZero()
    {
        // Arrange
        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddItemAsync(
                "CUST-001",
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldUpdateQuantity_WhenStockIsAvailable()
    {
        // Arrange
        var cart = CreateCart(quantity: 2);

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory());

        _cartRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateCartItemRequest
        {
            Quantity = 5
        };

        // Act
        var result = await _service.UpdateItemAsync(
            "CUST-001",
            "MI",
            "MI-JERSEY-001",
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(5, result.Items[0].Quantity);
        Assert.Equal(9995m, result.Subtotal);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        // Arrange
        var cart = CreateCart();

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateInventory(available: 5, reserved: 2));

        var request = new UpdateCartItemRequest
        {
            Quantity = 5
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateItemAsync(
                "CUST-001",
                "MI",
                "MI-JERSEY-001",
                request,
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveProductFromCart()
    {
        // Arrange
        var cart = CreateCart();

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _cartRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveItemAsync(
            "CUST-001",
            "MI",
            "MI-JERSEY-001",
            CancellationToken.None);

        // Assert
        Assert.Empty(cart.Items);

        _cartRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldThrowKeyNotFoundException_WhenCartDoesNotExist()
    {
        // Arrange
        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RemoveItemAsync(
                "CUST-001",
                "MI",
                "MI-JERSEY-001",
                CancellationToken.None));
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowInvalidOperationException_WhenCurrenciesDiffer()
    {
        // Arrange
        SetupProductAndInventory(
            product: CreateProduct(currency: "USD"));

        var cart = CreateCart();
        cart.Currency = "INR";

        _cartRepositoryMock
            .Setup(x => x.GetByCustomerIdAsync(
                "CUST-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var request = new AddToCartRequest
        {
            FranchiseId = "MI",
            ProductId = "MI-JERSEY-001",
            Quantity = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddItemAsync(
                "CUST-001",
                request,
                CancellationToken.None));
    }
}
