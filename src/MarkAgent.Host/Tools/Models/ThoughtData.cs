namespace MarkAgent.Host.Tools.Models;

public class ThoughtData
{
    public string thought { get; set; } = string.Empty;
    
    public int thoughtNumber { get; set; }
    
    public int totalThoughts { get; set; }
    
    public bool isRevision { get; set; } = false;
    
    public int? revisesThought { get; set; }
    
    public int? branchFromThought { get; set; }
    
    public string? branchId { get; set; }
    
    public bool needsMoreThoughts { get; set; } = false;
    
    public bool nextThoughtNeeded { get; set; } = true;
}