using MarkAgent.Domain.Enums;

namespace MarkAgent.Application.DTOs.Authentication;

public class AuthenticationResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime ExpiresAt { get; set; }
}