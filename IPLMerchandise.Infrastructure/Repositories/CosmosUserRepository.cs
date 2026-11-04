using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Domain.Entities;
using IPLMerchandise.Infrastructure.Documents;
using IPLMerchandise.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace IPLMerchandise.Infrastructure.Repositories;

public sealed class CosmosUserRepository : IUserRepository
{
    private readonly Container _container;

    public CosmosUserRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options)
    {
        _container = cosmosClient.GetContainer(
            options.Value.DatabaseName,
            "Users");
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.documentType = @type AND c.normalizedEmail = @email")
            .WithParameter("@type", "User")
            .WithParameter("@email", email.Trim().ToLowerInvariant());

        using var iterator = _container.GetItemQueryIterator<UserDocument>(
            query,
            requestOptions: new QueryRequestOptions
            {
                MaxItemCount = 1
            });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            var document = response.FirstOrDefault();

            if (document is not null)
                return MapToDomain(document);
        }

        return null;
    }

    public async Task<Domain.Entities.User?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<UserDocument>(
                userId,
                new PartitionKey(userId),
                cancellationToken: cancellationToken);

            return MapToDomain(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken)
    {
        var document = MapToDocument(user);

        try
        {
            await _container.CreateItemAsync(
                document,
                new PartitionKey(document.Id),
                cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }
    }

    private static Domain.Entities.User MapToDomain(UserDocument document)
    {
        return new Domain.Entities.User
        {
            Id = document.Id,
            FullName = document.FullName,
            Email = document.Email,
            PasswordHash = document.PasswordHash,
            IsActive = document.IsActive,
            CreatedAtUtc = document.CreatedAtUtc
        };
    }

    private static UserDocument MapToDocument(Domain.Entities.User user)
    {
        return new UserDocument
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            NormalizedEmail = user.Email.Trim().ToLowerInvariant(),
            PasswordHash = user.PasswordHash,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}