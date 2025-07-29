using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 客户端连接记录仓储实现
/// </summary>
public class ClientConnectionRepository : IClientConnectionRepository
{
    private readonly StatisticsDbContext _context;

    public ClientConnectionRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<ClientConnectionRecord> AddAsync(ClientConnectionRecord record)
    {
        _context.ClientConnectionRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<ClientConnectionRecord> UpdateAsync(ClientConnectionRecord record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        _context.ClientConnectionRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<ClientConnectionRecord?> GetBySessionIdAsync(string sessionId)
    {
        return await _context.ClientConnectionRecords
            .FirstOrDefaultAsync(r => r.SessionId == sessionId);
    }

    public async Task<List<ClientConnectionRecord>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ClientConnectionRecords
            .Where(r => r.ConnectionTime >= startDate && r.ConnectionTime < endDate)
            .OrderByDescending(r => r.ConnectionTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ClientConnectionRecord>> GetRecordsByClientNameAsync(string clientName, int skip = 0, int take = 100)
    {
        return await _context.ClientConnectionRecords
            .Where(r => r.ClientName == clientName)
            .OrderByDescending(r => r.ConnectionTime)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ClientConnectionRecord>> GetRecentConnectionsAsync(int count = 20)
    {
        return await _context.ClientConnectionRecords
            .OrderByDescending(r => r.ConnectionTime)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTotalConnectionCountAsync()
    {
        return await _context.ClientConnectionRecords.CountAsync();
    }

    public async Task<Dictionary<string, int>> GetActiveClientsStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ClientConnectionRecords.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(r => r.ConnectionTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.ConnectionTime < endDate.Value);

        return await query
            .GroupBy(r => r.ClientName)
            .Select(g => new { ClientName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientName, x => x.Count);
    }

    public async Task<Dictionary<string, Dictionary<string, int>>> GetClientVersionDistributionAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        var results = await _context.ClientConnectionRecords
            .Where(r => r.ConnectionTime >= startDate)
            .GroupBy(r => new { r.ClientName, r.ClientVersion })
            .Select(g => new { 
                ClientName = g.Key.ClientName, 
                Version = g.Key.ClientVersion ?? "Unknown", 
                Count = g.Count() 
            })
            .AsNoTracking()
            .ToListAsync();

        var distribution = new Dictionary<string, Dictionary<string, int>>();
        
        foreach (var result in results)
        {
            if (!distribution.ContainsKey(result.ClientName))
                distribution[result.ClientName] = new Dictionary<string, int>();
            
            distribution[result.ClientName][result.Version] = result.Count;
        }

        return distribution;
    }

    public async Task MarkAsDisconnectedAsync(string sessionId, DateTime disconnectionTime)
    {
        var record = await _context.ClientConnectionRecords
            .FirstOrDefaultAsync(r => r.SessionId == sessionId);

        if (record != null)
        {
            record.Status = ClientConnectionStatus.Disconnected;
            record.DisconnectionTime = disconnectionTime;
            record.ConnectionDurationSeconds = (long)(disconnectionTime - record.ConnectionTime).TotalSeconds;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}