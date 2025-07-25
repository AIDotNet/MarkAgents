using MarkAgent.Application.DTOs.McpService;

namespace MarkAgent.Application.Services;

public interface IMcpServiceManagementService
{
    Task<McpServiceDto> CreateMcpServiceAsync(CreateMcpServiceRequest request, CancellationToken cancellationToken = default);
    Task<McpServiceDto> UpdateMcpServiceAsync(Guid serviceId, UpdateMcpServiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteMcpServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<McpServiceDto?> GetMcpServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServiceDto>> GetAllMcpServicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServiceDto>> GetActiveMcpServicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<McpServiceDto>> GetAvailableServicesForUserAsync(string userKey, CancellationToken cancellationToken = default);
    Task InitializeDefaultServicesAsync(CancellationToken cancellationToken = default);
}