using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.Authentication;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}