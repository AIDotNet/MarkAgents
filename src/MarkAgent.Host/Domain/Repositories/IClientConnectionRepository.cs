using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 客户端连接记录仓储接口
/// </summary>
public interface IClientConnectionRepository
{
    /// <summary>
    /// 添加客户端连接记录
    /// </summary>
    Task<ClientConnectionRecord> AddAsync(ClientConnectionRecord record);

    /// <summary>
    /// 更新客户端连接记录
    /// </summary>
    Task<ClientConnectionRecord> UpdateAsync(ClientConnectionRecord record);

    /// <summary>
    /// 根据会话ID查找连接记录
    /// </summary>
    Task<ClientConnectionRecord?> GetBySessionIdAsync(string sessionId);

    /// <summary>
    /// 获取指定时间范围内的连接记录
    /// </summary>
    Task<List<ClientConnectionRecord>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取指定客户端的连接记录
    /// </summary>
    Task<List<ClientConnectionRecord>> GetRecordsByClientNameAsync(string clientName, int skip = 0, int take = 100);

    /// <summary>
    /// 获取最近的连接记录
    /// </summary>
    Task<List<ClientConnectionRecord>> GetRecentConnectionsAsync(int count = 20);

    /// <summary>
    /// 获取总连接次数
    /// </summary>
    Task<int> GetTotalConnectionCountAsync();

    /// <summary>
    /// 获取活跃客户端统计（按客户端名称分组）
    /// </summary>
    Task<Dictionary<string, int>> GetActiveClientsStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 获取客户端版本分布
    /// </summary>
    Task<Dictionary<string, Dictionary<string, int>>> GetClientVersionDistributionAsync(int days = 30);

    /// <summary>
    /// 标记连接为断开状态
    /// </summary>
    Task MarkAsDisconnectedAsync(string sessionId, DateTime disconnectionTime);
}