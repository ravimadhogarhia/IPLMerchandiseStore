using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Application.DTOs;

public sealed class ProductDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string FranchiseId { get; set; } = string.Empty;

    public string FranchiseName { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "INR";

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

}