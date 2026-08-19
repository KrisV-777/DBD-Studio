using System.Diagnostics.CodeAnalysis;

namespace DBDStudio.Models.Component
{
    [method: SetsRequiredMembers]
    public sealed class RuleConstruct(Rule underlying, bool isPrimordial = false)
        : Construct<Rule>(underlying, isPrimordial)
    {
        public string? SourceFilePath { get; init; }
    }
}
