namespace IPLMerchandise.Application.DTOs;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } =
        Array.Empty<T>();

    public string? ContinuationToken { get; set; }

    public int PageSize { get; set; }
}