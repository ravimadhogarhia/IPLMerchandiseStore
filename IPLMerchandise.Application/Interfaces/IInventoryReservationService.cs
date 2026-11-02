
namespace IPLMerchandise.Application.Interfaces;

public interface IInventoryReservationService
{
    Task ReserveAsync(
        string franchiseId,
        string productId,
        int quantity,
        CancellationToken cancellationToken);

    Task ReleaseAsync(
        string franchiseId,
        string productId,
        int quantity,
        CancellationToken cancellationToken);
}
