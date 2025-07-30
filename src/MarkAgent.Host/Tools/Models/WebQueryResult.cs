namespace MarkAgent.Host.Tools.Models;

public class WebQueryResult
{
    public string query { get; set; }
    
    public Results[] results { get; set; }
    
    public double response_time { get; set; }
}

public class Results
{
    public string url { get; set; }
    
    public string title { get; set; }
    
    public string content { get; set; }
    
    public double score { get; set; }
}

