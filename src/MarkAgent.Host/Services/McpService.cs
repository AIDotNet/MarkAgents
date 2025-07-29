using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// MCP服务实现
/// </summary>
public class McpService : IMcpService
{
    // TODO: 完整实现，这里先占位
    
    public Task<(bool Success, string Message, Conversation? Conversation)> CreateConversationAsync(string userKey, string? title = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Conversation?> GetActiveConversationAsync(string userKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, string Message)> EndConversationAsync(string userKey, Guid conversationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateConversationTokenUsageAsync(Guid conversationId, long tokenUsage, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Conversation>> GetUserConversationsAsync(string userKey, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(int Total, int Active, int Completed, int Interrupted, int Error)> GetConversationStatisticsAsync(string userKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, string Message)> ConfigureToolsAsync(string userKey, List<McpToolConfig> toolConfigs, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<McpToolConfig>> GetEnabledToolsAsync(string userKey, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, string Message)> ToggleToolAsync(string userKey, string toolName, bool isEnabled, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RecordToolUsageAsync(string userKey, Guid conversationId, string toolName, string? parameters, string? result, ExecutionStatus status, int executionTimeMs = 0, int tokensUsed = 0, string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, int>> GetToolUsageStatisticsAsync(string userKey, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
} 