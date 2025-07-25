using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.Todo;

public class CreateTodoRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Priority { get; set; } = 0;

    public DateTime? DueDate { get; set; }

    [Required]
    public Guid ConversationSessionId { get; set; }
}