using MarkAgent.Domain.Entities;
using MarkAgent.Domain.ValueObjects;

namespace MarkAgent.Domain.Repositories;

public interface IUserApiKeyRepository
{
    Task<UserApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserApiKey?> GetByApiKeyAsync(UserKey apiKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserApiKey>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserApiKey apiKey, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserApiKey apiKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserApiKey apiKey, CancellationToken cancellationToken = default);
    Task<bool> ApiKeyExistsAsync(UserKey apiKey, CancellationToken cancellationToken = default);
}