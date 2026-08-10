using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Persistence;

namespace DBDStudio.Core.Services
{
    public sealed class MockRuleService : IRuleService, IPersistable
    {
        private readonly List<Rule> _rules = [];

        public string PersistenceKey => "rules";
        public Type PersistenceStateType => typeof(RulePersistenceState);

        public object? SaveState()
        {
            return new RulePersistenceState {
                Rules = [.. _rules.Select(CloneRule)]
            };
        }

        public void RestoreState(object? state)
        {
            _rules.Clear();
            if (state is not RulePersistenceState persistenceState) {
                return;
            }

            foreach (var rule in persistenceState.Rules) {
                _rules.Add(CloneRule(rule));
            }
        }

        public IReadOnlyList<Rule> GetRules() => _rules;

        public void Add(Rule rule) => _rules.Add(CloneRule(rule));

        public void Update(Rule rule)
        {
            var existing = _rules.FirstOrDefault(x => x.Name == rule.Name);
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

        public void Remove(Rule rule)
        {
            var existing = _rules.FirstOrDefault(x => x.Name == rule.Name);
            if (existing is not null) {
                _rules.Remove(existing);
            }
        }

        private static Rule CloneRule(Rule rule)
        {
            var clone = new Rule {
                Name = rule.Name,
                FileName = rule.FileName,
                PriorityPreview = rule.PriorityPreview
            };

            foreach (var candidate in rule.TextureCandidates) {
                clone.TextureCandidates.Add(candidate);
            }

            foreach (var candidate in rule.BodySlideCandidates) {
                clone.BodySlideCandidates.Add(candidate);
            }

            foreach (var candidate in rule.RaceMenuCandidates) {
                clone.RaceMenuCandidates.Add(candidate);
            }

            foreach (var condition in rule.Conditions) {
                clone.Conditions.Add(condition);
            }

            return clone;
        }
    }
}
