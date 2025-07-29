using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 用户统计仓储实现类
/// </summary>
public class UserStatisticsRepository : BaseRepository<UserStatistics, Guid>, IUserStatisticsRepository
{
    public UserStatisticsRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<UserStatistics?> GetByUserAndDateAsync(Guid userId, DateTime date,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _dbSet.FirstOrDefaultAsync(
            s => s.UserId == userId && s.StatDate.Date == dateOnly,
            cancellationToken);
    }

    public async Task<UserStatistics?> GetByUserKeyAndDateAsync(Guid userKeyId, DateTime date,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _dbSet.FirstOrDefaultAsync(
            s => s.UserKeyId == userKeyId && s.StatDate.Date == dateOnly,
            cancellationToken);
    }

    public async Task<List<UserStatistics>> GetByUserAndDateRangeAsync(Guid userId, DateTime startDate,
        DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.UserId == userId && s.StatDate >= startDate && s.StatDate <= endDate)
            .OrderBy(s => s.StatDate)
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementTodosCreatedAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1,
        CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;

        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == dateOnly)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TodosCreated, x => x.TodosCreated + count),
                cancellationToken);
    }

    public async Task IncrementTodosCompletedAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == date.Date)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TodosCompleted, x => x.TodosCompleted + count),
                cancellationToken);
    }

    public async Task IncrementConversationsCountAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == date.Date)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ConversationsCount, x => x.ConversationsCount + count),
                cancellationToken);
    }

    public async Task IncrementTokenUsageAsync(Guid userId, Guid userKeyId, DateTime date, long tokens,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == date.Date)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TotalTokensUsed, x => x.TotalTokensUsed + tokens),
                cancellationToken);
    }

    public async Task IncrementApiCallsAsync(Guid userId, Guid userKeyId, DateTime date, long calls = 1,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == date.Date)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ApiCallsCount, x => x.ApiCallsCount + calls),
                cancellationToken);
    }

    public async Task IncrementOnlineTimeAsync(Guid userId, Guid userKeyId, DateTime date, int minutes,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.Where(x => x.UserId == userId && x.StatDate.Date == date.Date)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TotalOnlineMinutes, x => x.TotalOnlineMinutes + minutes),
                cancellationToken);
    }
}