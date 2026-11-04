
using IPLMerchandise.Domain.Entities;

namespace IPLMerchandise.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task CreateAsync(
        User user,
        CancellationToken cancellationToken);
}
