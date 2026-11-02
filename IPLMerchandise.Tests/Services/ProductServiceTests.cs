
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IPLMerchandise.Application.DTOs;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Application.Services;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Domain.Enums;
using Moq;
using Xunit;

namespace IPLMerchandise.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object);
    }

    private static Product CreateProduct()
    {
        return new Product
        {
            Id = "MI-JERSEY-001",
            Name = "Mumbai Indians Jersey",
            Description = "Official-style Mumbai Indians jersey",
            FranchiseId = "MI",
            FranchiseName = "Mumbai Indians",
            Category = ProductCategory.Jersey,
            Price = 1999m,
            Currency = "INR",
            ImageUrl = "https://example.com/mi-jersey.jpg",
            IsActive = true
        };
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var product = CreateProduct();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(
            "MI",
            "MI-JERSEY-001",
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MI-JERSEY-001", result.Id);
        Assert.Equal("Mumbai Indians Jersey", result.Name);
        Assert.Equal("MI", result.FranchiseId);
        Assert.Equal(ProductCategory.Jersey, result.Category);
        Assert.Equal(1999m, result.Price);
        Assert.Equal("INR", result.Currency);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "UNKNOWN-PRODUCT",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetByIdAsync(
            "MI",
            "UNKNOWN-PRODUCT",
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldNormalizeFranchiseId()
    {
        // Arrange
        var product = CreateProduct();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(
            " mi ",
            " MI-JERSEY-001 ",
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
                "MI",
                "MI-JERSEY-001",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenFranchiseIdIsInvalid(
        string? franchiseId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetByIdAsync(
                franchiseId!,
                "MI-JERSEY-001",
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenProductIdIsInvalid(
        string? productId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetByIdAsync(
                "MI",
                productId!,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(),
            new()
            {
                Id = "CSK-CAP-001",
                Name = "CSK Cap",
                Description = "Chennai cap",
                FranchiseId = "CSK",
                FranchiseName = "Chennai Super Kings",
                Category = ProductCategory.Cap,
                Price = 499m,
                Currency = "INR",
                ImageUrl = "https://example.com/csk-cap.jpg",
                IsActive = true
            }
        };

        var repositoryResult = new PagedResult<Product>
        {
            Items = products,
            ContinuationToken = "next-page-token",
            PageSize = 10
        };

        _repositoryMock
            .Setup(x => x.SearchAsync(
                null,
                null,
                null,
                10,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _service.SearchAsync(
            null,
            null,
            null,
            10,
            null,
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("next-page-token", result.ContinuationToken);
        Assert.Equal(10, result.PageSize);

        Assert.Equal("MI-JERSEY-001", result.Items[0].Id);
        Assert.Equal("CSK-CAP-001", result.Items[1].Id);

        Assert.Equal(1999m, result.Items[0].Price);
        Assert.Equal(ProductCategory.Cap, result.Items[1].Category);
    }

    [Fact]
    public async Task SearchAsync_ShouldPassSearchAndFranchiseFilters()
    {
        // Arrange
        var repositoryResult = new PagedResult<Product>
        {
            Items = new List<Product> { CreateProduct() },
            PageSize = 10
        };

        _repositoryMock
            .Setup(x => x.SearchAsync(
                "jersey",
                "MI",
                ProductCategory.Jersey,
                10,
                "token-123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _service.SearchAsync(
            "jersey",
            " mi ",
            ProductCategory.Jersey,
            10,
            "token-123",
            CancellationToken.None);

        // Assert
        Assert.Single(result.Items);

        _repositoryMock.Verify(
            x => x.SearchAsync(
                "jersey",
                "MI",
                ProductCategory.Jersey,
                10,
                "token-123",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public async Task SearchAsync_ShouldThrowArgumentException_WhenPageSizeIsInvalid(
        int pageSize)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SearchAsync(
                null,
                null,
                null,
                pageSize,
                null,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.SearchAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<ProductCategory?>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldThrowArgumentException_WhenFranchiseIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.SearchAsync(
                null,
                "   ",
                null,
                10,
                null,
                CancellationToken.None));

        _repositoryMock.Verify(
            x => x.SearchAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<ProductCategory?>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyResults_WhenNoProductsMatch()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.SearchAsync(
                "nonexistent",
                null,
                null,
                10,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Product>
            {
                Items = new List<Product>(),
                PageSize = 10,
                ContinuationToken = null
            });

        // Act
        var result = await _service.SearchAsync(
            "nonexistent",
            null,
            null,
            10,
            null,
            CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Null(result.ContinuationToken);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task SearchAsync_ShouldNormalizeFranchiseIdToUppercase()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.SearchAsync(
                null,
                "CSK",
                null,
                10,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Product>
            {
                Items = new List<Product>(),
                PageSize = 10
            });

        // Act
        await _service.SearchAsync(
            null,
            " csk ",
            null,
            10,
            null,
            CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            x => x.SearchAsync(
                null,
                "CSK",
                null,
                10,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
