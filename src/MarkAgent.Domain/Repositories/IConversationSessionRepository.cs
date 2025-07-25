using MarkAgent.Domain.Entities;

namespace MarkAgent.Domain.Repositories;

public interface IConversationSessionRepository
{
    Task<ConversationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(ConversationSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConversationSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(ConversationSession session, CancellationToken cancellationToken = default);
    Task<int> GetTotalSessionCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}