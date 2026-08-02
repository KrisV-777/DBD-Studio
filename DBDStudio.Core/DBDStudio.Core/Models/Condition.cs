namespace DBDStudio.Core.Models;

public sealed class Condition
{
    public string Type { get; set; } = string.Empty;
    public string Operator { get; set; } = "==";
    public string Value { get; set; } = string.Empty;
}
