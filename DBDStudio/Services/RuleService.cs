using System.Collections.ObjectModel;
using System.Text.Json;
using DBDStudio.Converter.Json;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Utility;

namespace DBDStudio.Services
{
    public sealed class RuleService : IRuleService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        public ObservableCollection<RuleConstruct> Rules { get; } = [];

        public RuleService(ApplicationSettings settings)
        {
            _settings = settings;
            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or nameof(ApplicationSettings.ModsFolder)) {
                    ResetRuleList();
                }
            };
        }

        public void ResetRuleList(IReadOnlyList<Rule>? rules = null)
        {
            var mergedRules = Rules
                .Select(rule => rule.Underlying)
                .Concat(rules ?? [])
                .GroupBy(rule => rule.Uid)
                .Select(group => group
                    .OrderByDescending(rule => rule.LastUpdatedUtc)
                    .First())
                .ToArray();

            Rules.Clear();

            foreach (var rule in mergedRules) {
                var hasPublishedPath = !string.IsNullOrWhiteSpace(rule.LastPublishedPath);
                Rules.Add(new RuleConstruct(rule, isPrimordial: hasPublishedPath));
            }
        }

        public RuleConstruct EmplaceNew(string? withName = null)
        {
            var newRule = CreateNewRule(withName);
            Rules.Add(newRule);
            return newRule;
        }

        public void Remove(RuleConstruct rule)
        {
            if (rule.IsPrimordialAny()) {
                throw new InvalidOperationException("Cannot remove a primordial rule.");
            }
            Rules.Remove(rule);
        }

        public void Reset(RuleConstruct rule)
        {
            if (!rule.Is(ConstructState.Modified)) {
                throw new InvalidOperationException("Cannot reset a non-modified primordial rule.");
            }
            rule.Reset();
        }

        public void Save(RuleConstruct rule)
        {
            if (rule.Is(ConstructState.Ephemeral)) {
                throw new InvalidOperationException("Cannot save an ephemeral rule.");
            }

            var sourcePath = rule.Underlying.LastPublishedPath
                ?? throw new InvalidOperationException("Cannot save a rule without a published source file path.");
            var normalizedPath = NormalizePath(sourcePath);
            WriteRuleToDisk(rule.Underlying, normalizedPath);

            var replacement = CreateSavedRule(rule.Underlying, normalizedPath);
            var index = Rules.IndexOf(rule);
            if (index >= 0) {
                Rules[index] = replacement;
                return;
            }

            Rules.Add(replacement);
        }

        public void SaveAs(RuleConstruct rule, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) {
                return;
            }

            var normalizedPath = NormalizePath(filePath);
            var sourceFileExisted = File.Exists(normalizedPath);
            WriteRuleToDisk(rule.Underlying, normalizedPath);

            var persistedRule = CreateSavedRule(rule.Underlying, normalizedPath);
            var selectedIndex = Rules.IndexOf(rule);
            var existingSourceIndex = sourceFileExisted
                ? Rules
                    .Select((existingRule, index) => new { existingRule, index })
                    .FirstOrDefault(entry => PathsEqual(entry.existingRule.Underlying.LastPublishedPath, normalizedPath))
                    ?.index ?? -1
                : -1;

            var replacementIndex = existingSourceIndex >= 0 ? existingSourceIndex : selectedIndex;
            if (replacementIndex >= 0) {
                Rules[replacementIndex] = persistedRule;

                if (!ReferenceEquals(rule, persistedRule)
                    && selectedIndex >= 0
                    && selectedIndex != replacementIndex
                    && Rules.Contains(rule)) {
                    Rules.Remove(rule);
                }
                return;
            }

            Rules.Add(persistedRule);
        }

        private static RuleConstruct CreateSavedRule(Rule sourceRule, string sourcePath)
        {
            var current = new Rule();
            current.Import(sourceRule);
            current.RestoreLastUpdatedUtc(sourceRule.LastUpdatedUtc);
            current.LastPublishedPath = sourcePath;
            return new RuleConstruct(current, isPrimordial: true);
        }

        private RuleConstruct CreateNewRule(string? baseName = null)
        {
            return new RuleConstruct(new Rule {
                Name = UniqueNameGenerator.CreateUniqueName(
                    baseName,
                    "New Rule",
                    Rules.Select(existingRule => existingRule.Name)),
            });
        }

        private static void WriteRuleToDisk(Rule rule, string filePath)
        {
            var outputPath = NormalizePath(filePath);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory)) {
                Directory.CreateDirectory(directory);
            }

            var exportedRule = new Rule();
            exportedRule.Import(rule);
            exportedRule.RestoreLastUpdatedUtc(rule.LastUpdatedUtc);
            exportedRule.LastPublishedPath = null;

            var jsonConfig = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Publish);
            var json = JsonSerializer.Serialize(exportedRule, jsonConfig);
            File.WriteAllText(outputPath, json);
        }

        private static string NormalizePath(string filePath)
        {
            var normalizedPath = EnsureJsonExtension(filePath);
            return Path.GetFullPath(normalizedPath);
        }

        private static string EnsureJsonExtension(string filePath)
        {
            return filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? filePath
                : filePath + ".json";
        }

        private static bool PathsEqual(string? left, string? right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) {
                return false;
            }

            try {
                return string.Equals(
                    Path.GetFullPath(left),
                    Path.GetFullPath(right),
                    StringComparison.OrdinalIgnoreCase);
            } catch (Exception) {
                return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
            }
        }

        #region IPersistable

        public string PersistenceKey => "rules";
        public Type PersistenceStateType => typeof(List<Rule>);

        public object? SaveState() => Rules.Select(rule => rule.Underlying).ToList();

        public void RestoreState(object? state)
        {
            if (state is not List<Rule> rules) {
                return;
            }
            ResetRuleList(rules);
        }

        #endregion
    }
}
