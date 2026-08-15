using DBDStudio.Models;

namespace DBDStudio.Interfaces
{
    public interface IRuleResolutionService
    {
        int GetDerivedPriority(Rule rule);
        Rule? ResolveWinningRule(IEnumerable<Rule> rules, AssignmentCategory category);
        string? ResolveWinningCandidate(Rule rule, AssignmentCategory category);
    }
}
