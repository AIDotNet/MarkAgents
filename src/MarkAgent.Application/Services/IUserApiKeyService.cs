using MarkAgent.Application.DTOs.ApiKey;

namespace MarkAgent.Application.Services;

public interface IUserApiKeyService
{
    Task<UserApiKeyDto> CreateApiKeyAsync(CreateApiKeyRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<UserApiKeyDto> UpdateApiKeyAsync(Guid apiKeyId, UpdateApiKeyRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteApiKeyAsync(Guid apiKeyId, Guid userId, CancellationToken cancellationToken = default);
    Task<UserApiKeyDto?> GetApiKeyAsync(Guid apiKeyId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserApiKeyDto>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserApiKeyDto?> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task UpdateMcpServicesAsync(Guid apiKeyId, List<McpServiceSelectionDto> services, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessToMcpServiceAsync(string apiKey, string serviceName, CancellationToken cancellationToken = default);
}