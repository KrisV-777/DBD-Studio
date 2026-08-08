using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services
{
    public sealed class RuleResolutionService : IRuleResolutionService
    {
        private readonly IConditionRegistryService _conditionRegistryService;
        private readonly Random _random = new();

        public RuleResolutionService(IConditionRegistryService conditionRegistryService)
        {
            _conditionRegistryService = conditionRegistryService;
        }

        public int GetDerivedPriority(Rule rule)
        {
            if (rule.Conditions.Count == 0) {
                return 0;
            }

            var maxPriority = 0;
            foreach (var condition in rule.Conditions)
            {
                var definition = _conditionRegistryService.FindByName(condition.Type);
                var priority = definition?.Priority ?? 0;
                if (priority > maxPriority) {
                    maxPriority = priority;
                }
            }

            return maxPriority;
        }

        public Rule? ResolveWinningRule(IEnumerable<Rule> rules, AssignmentCategory category)
        {
            var candidates = rules
                .Where(rule => ResolveCandidates(rule, category).Count > 0)
                .OrderBy(rule => GetDerivedPriority(rule))
                .ThenBy(rule => rule.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return candidates.Count == 0 ? null : candidates[^1];
        }

        public string? ResolveWinningCandidate(Rule rule, AssignmentCategory category)
        {
            var candidates = ResolveCandidates(rule, category);
            if (candidates.Count == 0) {
                return null;
            }

            return candidates[_random.Next(candidates.Count)];
        }

        private static IReadOnlyList<string> ResolveCandidates(Rule rule, AssignmentCategory category)
        {
            return category switch
            {
                AssignmentCategory.Texture => rule.TextureCandidates,
                AssignmentCategory.BodySlide => rule.BodySlideCandidates,
                AssignmentCategory.RaceMenu => rule.RaceMenuCandidates,
                _ => []
            };
        }
    }
}
