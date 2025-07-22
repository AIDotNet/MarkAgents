using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MarkAgents.Functions.Model;

public sealed class TodoInputItem
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("status")]
    public required TodoInputItemStatus Status { get; set; }

    [JsonPropertyName("priority")]
    public required Priority Priority { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TodoInputItemStatus
{
    [Description("pending")] Pending,

    [Description("in_progress")] InProgress,

    [Description("completed")] Completed,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Priority
{
    [Description("low")] Low,

    [Description("medium")] Medium,

    [Description("high")] High,
}