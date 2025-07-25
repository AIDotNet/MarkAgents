using MarkAgent.Application.DTOs.Statistics;

namespace MarkAgent.Application.Services;

public interface IStatisticsService
{
    Task<UserStatisticsDto> GetUserStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
    Task UpdateUserStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
}