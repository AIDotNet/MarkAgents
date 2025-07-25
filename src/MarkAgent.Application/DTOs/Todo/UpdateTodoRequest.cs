using System.ComponentModel.DataAnnotations;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Application.DTOs.Todo;

public class UpdateTodoRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public TodoStatus? Status { get; set; }

    public int? Priority { get; set; }

    public DateTime? DueDate { get; set; }
}