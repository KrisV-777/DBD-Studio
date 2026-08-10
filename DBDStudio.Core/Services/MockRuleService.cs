using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services
{
    public sealed class MockRuleService : IRuleService
    {
        private readonly IWorkspaceService _workspaceService;

        public MockRuleService(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        public IReadOnlyList<Rule> GetRules() => _workspaceService.Rules;

        public void Add(Rule rule) => throw new NotImplementedException("MockRuleService does not support adding rules.");

        public void Update(Rule rule)
        {
            var existing = _workspaceService.Rules.FirstOrDefault(x => x.Name == rule.Name);
            if (existing is null) {
                return;
            }

            existing.FileName = rule.FileName;
            existing.TextureCandidates.Clear();
            foreach (var item in rule.TextureCandidates) {
                existing.TextureCandidates.Add(item);
            }

            existing.BodySlideCandidates.Clear();
            foreach (var item in rule.BodySlideCandidates) {
                existing.BodySlideCandidates.Add(item);
            }

            existing.RaceMenuCandidates.Clear();
            foreach (var item in rule.RaceMenuCandidates) {
                existing.RaceMenuCandidates.Add(item);
            }

            existing.Conditions.Clear();
            foreach (var condition in rule.Conditions) {
                existing.Conditions.Add(condition);
            }
        }

        public void Remove(Rule rule) => throw new NotImplementedException("MockRuleService does not support removing rules.");
    }
}
