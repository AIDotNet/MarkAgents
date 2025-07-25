using System.ComponentModel.DataAnnotations;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Application.DTOs.User;

public class UpdateUserRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public UserRole Role { get; set; }
}