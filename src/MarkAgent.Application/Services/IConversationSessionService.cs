using MarkAgent.Domain.Entities;

namespace MarkAgent.Application.Services;

public interface IConversationSessionService
{
    Task<ConversationSession> CreateSessionAsync(string sessionName, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationSession?> GetSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConversationSession>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationSession> EndSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationSession> UpdateSessionNameAsync(Guid sessionId, string sessionName, Guid userId, CancellationToken cancellationToken = default);
}