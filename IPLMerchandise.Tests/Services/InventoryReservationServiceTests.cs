
using System;
using System.Threading;
using System.Threading.Tasks;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Domain.Entities;
using Moq;
using Xunit;

namespace IPLMerchandise.Tests.Services;

public class InventoryReservationServiceTests
{
    private readonly Mock<IInventoryRepository> _repositoryMock;
    private readonly InventoryReservationService _service;

    public InventoryReservationServiceTests()
    {
        _repositoryMock = new Mock<IInventoryRepository>();

        _service = new InventoryReservationService(
            _repositoryMock.Object);
    }

    private static Inventory CreateInventory(
        int available = 100,
        int reserved = 10)
    {
        return new Inventory
        {
            Id = "MI-MI-JERSEY-001",
            ProductId = "MI-JERSEY-001",
            FranchiseId = "MI",
            AvailableQuantity = available,
            ReservedQuantity = reserved,
            UpdatedAtUtc = DateTime.UtcNow,
            ETag = "test-etag"
        };
    }

    [Fact]
    public async Task ReserveAsync_ShouldIncreaseReservedQuantity_WhenStockIsAvailable()
    {
        // Arrange
        var inventory = CreateInventory(100, 10);

        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        Inventory? savedInventory = null;

        _repositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()))
            .Callback<Inventory, CancellationToken>(
                (value, _) => savedInventory = value)
            .Returns(Task.CompletedTask);

        // Act
        await _service.ReserveAsync(
            "MI",
            "MI-JERSEY-001",
            5,
            CancellationToken.None);

        // Assert
        Assert.NotNull(savedInventory);
        Assert.Equal(15, savedInventory.ReservedQuantity);

        _repositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReserveAsync_ShouldThrowInvalidOperationException_WhenStockIsInsufficient()
    {
        // Arrange
        var inventory = CreateInventory(100, 95);

        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                10,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReserveAsync_ShouldRetry_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var upsertAttempts = 0;

        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateInventory(100, 10));

        _repositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()))
            .Callback<Inventory, CancellationToken>(
                (_, _) =>
                {
                    upsertAttempts++;

                    if (upsertAttempts == 1)
                    {
                        throw new InvalidOperationException(
                            "Inventory was modified by another request.");
                    }
                })
            .Returns(Task.CompletedTask);

        // Act
        await _service.ReserveAsync(
            "MI",
            "MI-JERSEY-001",
            5,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, upsertAttempts);

        _repositoryMock.Verify(
            x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ReserveAsync_ShouldFailAfterFiveAttempts_WhenConcurrencyConflictsContinue()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateInventory(100, 10));

        _repositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(
                "Inventory was modified by another request."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                5,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()),
            Times.Exactly(5));

        _repositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(5));
    }

    [Fact]
    public async Task ReleaseAsync_ShouldDecreaseReservedQuantity_WhenStockIsReserved()
    {
        // Arrange
        var inventory = CreateInventory(100, 20);

        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        Inventory? savedInventory = null;

        _repositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()))
            .Callback<Inventory, CancellationToken>(
                (value, _) => savedInventory = value)
            .Returns(Task.CompletedTask);

        // Act
        await _service.ReleaseAsync(
            "MI",
            "MI-JERSEY-001",
            5,
            CancellationToken.None);

        // Assert
        Assert.NotNull(savedInventory);
        Assert.Equal(15, savedInventory.ReservedQuantity);

        _repositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReleaseAsync_ShouldThrowInvalidOperationException_WhenQuantityExceedsReservedStock()
    {
        // Arrange
        var inventory = CreateInventory(100, 5);

        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inventory);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ReleaseAsync(
                "MI",
                "MI-JERSEY-001",
                10,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<Inventory>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReserveAsync_ShouldThrowKeyNotFoundException_WhenInventoryDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByProductAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Inventory?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                5,
                CancellationToken.None));
    }

    [Fact]
    public async Task ReserveAsync_ShouldThrowArgumentException_WhenQuantityIsZero()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ReserveAsync(
                "MI",
                "MI-JERSEY-001",
                0,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.GetByProductAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
