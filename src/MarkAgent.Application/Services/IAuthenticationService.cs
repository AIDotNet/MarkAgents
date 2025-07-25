using MarkAgent.Application.DTOs.Authentication;

namespace MarkAgent.Application.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<string> GenerateJwtTokenAsync(Guid userId, string email, string userKey);
}