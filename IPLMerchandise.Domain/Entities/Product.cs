using IPLMerchandise.Domain.Enums;

namespace IPLMerchandise.Domain.Entities;

public sealed class Product
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

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}