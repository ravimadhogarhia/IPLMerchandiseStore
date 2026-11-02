using IPLMerchandise.Domain.Enums;
using IPLMerchandise.Infrastructure.Documents;

namespace IPLMerchandise.Infrastructure.Seed;

public static class ProductSeedData
{
    public static IReadOnlyList<ProductDocument> GetProducts()
    {
        var now = DateTime.UtcNow;

        return new List<ProductDocument>
        {
            Create(
                "MI-JERSEY-001",
                "MI",
                "Mumbai Indians Jersey",
                "Official blue Mumbai Indians jersey",
                "Mumbai Indians",
                ProductCategory.Jersey,
                1999,
                now),

            Create(
                "CSK-CAP-001",
                "CSK",
                "CSK Official Cap",
                "Yellow Chennai Super Kings cap",
                "Chennai Super Kings",
                ProductCategory.Cap,
                499,
                now),

            Create(
                "RCB-FLAG-001",
                "RCB",
                "RCB Supporter Flag",
                "Red and black Royal Challengers supporter flag",
                "Royal Challengers Bengaluru",
                ProductCategory.Flag,
                299,
                now),

            Create(
                "KKR-PHOTO-001",
                "KKR",
                "KKR Signed Photograph",
                "Kolkata Knight Riders collectible photograph",
                "Kolkata Knight Riders",
                ProductCategory.Collectibles,
                4999,
                now),

            Create(
                "SRH-JERSEY-001",
                "SRH",
                "SRH Orange Jersey",
                "Orange Sunrisers Hyderabad supporter jersey",
                "Sunrisers Hyderabad",
                ProductCategory.Jersey,
                1799,
                now),

            Create(
                "RR-CAP-001",
                "RR",
                "Rajasthan Royals Cap",
                "Rajasthan Royals official supporter cap",
                "Rajasthan Royals",
                ProductCategory.Cap,
                599,
                now),

            Create(
                "DC-JERSEY-001",
                "DC",
                "Delhi Capitals Jersey",
                "Delhi Capitals blue supporter jersey",
                "Delhi Capitals",
                ProductCategory.Jersey,
                1699,
                now),

            Create(
                "GT-FLAG-001",
                "GT",
                "Gujarat Titans Flag",
                "Gujarat Titans supporter flag",
                "Gujarat Titans",
                ProductCategory.Flag,
                349,
                now)
        };
    }

    private static ProductDocument Create(
        string id,
        string franchiseId,
        string name,
        string description,
        string franchiseName,
        ProductCategory category,
        decimal price,
        DateTime now)
    {
        return new ProductDocument
        {
            Id = id,
            FranchiseId = franchiseId,
            Name = name,
            Description = description,
            FranchiseName = franchiseName,
            Category = category,
            Price = price,
            Currency = "INR",
            ImageUrl = string.Empty,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }
}