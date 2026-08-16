using System.Collections.Generic;
using System.Linq;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Utility.Persistence;

namespace DBDStudio.Services
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

            existing.TextureCandidates.Clear();
            foreach (var item in rule.TextureCandidates) {
                existing.TextureCandidates.Add(item);
            }

            existing.BodySlideCandidates.Clear();
            foreach (var item in rule.BodySlideCandidates) {
                existing.BodySlideCandidates.Add(item);
            }

            existing.RaceMenuCandidate = rule.RaceMenuCandidate;

            existing.Conditions.Clear();
            foreach (var condition in rule.Conditions) {
                existing.Conditions.Add(condition.DeepClone());
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
                RaceMenuCandidate = rule.RaceMenuCandidate
            };

            foreach (var candidate in rule.TextureCandidates) {
                clone.TextureCandidates.Add(candidate);
            }

            foreach (var candidate in rule.BodySlideCandidates) {
                clone.BodySlideCandidates.Add(candidate);
            }

            foreach (var condition in rule.Conditions) {
                clone.Conditions.Add(condition.DeepClone());
            }

            return clone;
        }
    }
}
