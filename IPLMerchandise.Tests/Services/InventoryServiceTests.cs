
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using IPLMerchandise.Application.DTOs.Inventory;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Tests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly InventoryService _inventoryService;

    public InventoryServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();

        _inventoryService = new InventoryService(
            _inventoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldReturnInventory_WhenInventoryExists()
    {
        // Arrange
        var inventory = new Inventory
        {
            Id = "MI-MI-JERSEY-001",
            ProductId = "MI-JERSEY-001",
            FranchiseId = "MI",
            AvailableQuantity = 100,
            ReservedQuantity = 10,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act
        var result = await _inventoryService.GetInventoryAsync(
            "MI",
            "MI-JERSEY-001",
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MI-JERSEY-001", result.ProductId);
        Assert.Equal("MI", result.FranchiseId);
        Assert.Equal(100, result.AvailableQuantity);
        Assert.Equal(10, result.ReservedQuantity);
        Assert.Equal(90, result.InStockQuantity);
        Assert.True(result.IsInStock);

        _inventoryRepositoryMock.Verify(
            x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldThrowKeyNotFoundException_WhenInventoryDoesNotExist()
    {
        // Arrange
        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _inventoryService.GetInventoryAsync(
                "MI",
                "MI-JERSEY-001",
                CancellationToken.None));
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldThrowInvalidOperationException_WhenReservedQuantityExceedsAvailableQuantity()
    {
        // Arrange
        var inventory = new Inventory
        {
            ProductId = "MI-JERSEY-001",
            FranchiseId = "MI",
            AvailableQuantity = 10,
            ReservedQuantity = 15
        };

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _inventoryService.GetInventoryAsync(
                "MI",
                "MI-JERSEY-001",
                CancellationToken.None));
    }

    [Fact]
    public async Task GetInventoryAsync_ShouldReturnOutOfStock_WhenAvailableQuantityIsZero()
    {
        // Arrange
        var inventory = new Inventory
        {
            ProductId = "MI-JERSEY-001",
            FranchiseId = "MI",
            AvailableQuantity = 0,
            ReservedQuantity = 0
        };

        _inventoryRepositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act
        var result = await _inventoryService.GetInventoryAsync(
            "MI",
            "MI-JERSEY-001",
            CancellationToken.None);

        // Assert
        Assert.Equal(0, result.InStockQuantity);
        Assert.False(result.IsInStock);
    }
}
