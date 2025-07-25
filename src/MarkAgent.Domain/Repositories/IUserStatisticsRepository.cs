using MarkAgent.Domain.Entities;

namespace MarkAgent.Domain.Repositories;

public interface IUserStatisticsRepository
{
    Task<UserStatistics?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserStatistics statistics, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserStatistics statistics, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserStatistics statistics, CancellationToken cancellationToken = default);
}