using MarkAgent.Domain.Entities;

namespace MarkAgent.Domain.Repositories;

public interface IMcpServiceRepository
{
    Task<McpService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<McpService?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<McpService>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<McpService>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<McpService>> GetByUserKeyAsync(string userKey, CancellationToken cancellationToken = default);
    Task AddAsync(McpService service, CancellationToken cancellationToken = default);
    Task UpdateAsync(McpService service, CancellationToken cancellationToken = default);
    Task DeleteAsync(McpService service, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}