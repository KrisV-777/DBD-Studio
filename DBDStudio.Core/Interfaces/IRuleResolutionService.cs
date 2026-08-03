using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces;

public interface IRuleResolutionService
{
    int GetDerivedPriority(Rule rule);
    Rule? ResolveWinningRule(IEnumerable<Rule> rules, AssignmentCategory category);
    string? ResolveWinningCandidate(Rule rule, AssignmentCategory category);
}
