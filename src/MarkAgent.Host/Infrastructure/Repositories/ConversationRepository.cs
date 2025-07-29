using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 对话仓储实现类
/// </summary>
public class ConversationRepository : BaseRepository<Conversation,Guid>, IConversationRepository
{
    public ConversationRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.UserKey)
            .Include(c => c.Todos)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetByUserKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.UserKey)
            .Include(c => c.Todos)
            .Where(c => c.UserKey.Key == userKey)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetByStatusAsync(ConversationStatus status, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.Status == status);
        
        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }
        
        return await query
            .Include(c => c.UserKey)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Conversation>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.UserKey)
            .Include(c => c.Todos)
            .Where(c => c.UserId == userId && c.Status == ConversationStatus.Active)
            .OrderByDescending(c => c.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetActiveByUserKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.UserKey)
            .Include(c => c.Todos)
            .FirstOrDefaultAsync(c => c.UserKey.Key == userKey && c.Status == ConversationStatus.Active, cancellationToken);
    }

    public async Task<(int Total, int Active, int Completed, int Interrupted, int Error)> GetConversationStatsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _dbSet.Where(c => c.UserId == userId).ToListAsync(cancellationToken);
        return (
            conversations.Count,
            conversations.Count(c => c.Status == ConversationStatus.Active),
            conversations.Count(c => c.Status == ConversationStatus.Completed),
            conversations.Count(c => c.Status == ConversationStatus.Interrupted),
            conversations.Count(c => c.Status == ConversationStatus.Error)
        );
    }

    public async Task<int> GetConversationCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.StartedAt >= startDate && c.StartedAt <= endDate);
        
        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }

    public async Task EndConversationAsync(Guid conversationId, ConversationStatus status = ConversationStatus.Completed, CancellationToken cancellationToken = default)
    {
        var conversation = await GetByIdAsync(conversationId, cancellationToken);
        if (conversation is { Status: ConversationStatus.Active })
        {
            conversation.Status = status;
            conversation.EndedAt = DateTime.UtcNow;
            if (conversation.StartedAt != default)
            {
                conversation.DurationSeconds = (int)(conversation.EndedAt.Value - conversation.StartedAt).TotalSeconds;
            }
            await UpdateAsync(conversation, cancellationToken);
        }
    }

    public async Task EndActiveConversationsByUserAsync(Guid userId, ConversationStatus status = ConversationStatus.Completed, CancellationToken cancellationToken = default)
    {
        var activeConversations = await GetActiveByUserAsync(userId, cancellationToken);
        foreach (var conversation in activeConversations)
        {
            await EndConversationAsync(conversation.Id, status, cancellationToken);
        }
    }

} 