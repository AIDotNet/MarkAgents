namespace MarkAgent.Host.Tools.Models;

public class MentalModelData
{
    public MentalModelName ModelName { get; set; }

    public string Problem { get; set; } = string.Empty;

    public string[] Steps { get; set; } = [];

    public string Reasoning { get; set; } = string.Empty;

    public string Conclusion { get; set; } = string.Empty;
}

public enum MentalModelName
{
    FirstPrinciples,

    OpportunityCost,

    ErrorPropagation,

    RubberDuck,

    ParetoPrinciple,

    OccamsRazor,
}