using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 工具使用记录仓储实现
/// </summary>
public class ToolUsageRepository : IToolUsageRepository
{
    private readonly StatisticsDbContext _context;

    public ToolUsageRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<ToolUsageRecord> AddAsync(ToolUsageRecord record)
    {
        _context.ToolUsageRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<List<ToolUsageRecord>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ToolUsageRecords
            .Where(r => r.StartTime >= startDate && r.StartTime < endDate)
            .OrderByDescending(r => r.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ToolUsageRecord>> GetRecordsByToolNameAsync(string toolName, int skip = 0, int take = 100)
    {
        return await _context.ToolUsageRecords
            .Where(r => r.ToolName == toolName)
            .OrderByDescending(r => r.StartTime)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalUsageCountAsync()
    {
        return await _context.ToolUsageRecords.CountAsync();
    }

    public async Task<List<ToolUsageRecord>> GetRecentRecordsAsync(int count = 10)
    {
        return await _context.ToolUsageRecords
            .OrderByDescending(r => r.StartTime)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetUsageStatsByToolAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ToolUsageRecords.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(r => r.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.StartTime < endDate.Value);

        return await query
            .GroupBy(r => r.ToolName)
            .Select(g => new { ToolName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ToolName, x => x.Count);
    }
}