using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private const string RulesDirectoryInfix = "SKSE/DBD/Rules/";

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
            var oldRules = Rules
                .Select(rule => rule.Underlying)
                .Concat(rules ?? [])
                .DistinctBy(rule => rule.Uid)
                .ToArray();

            Rules.Clear();

            try {
                foreach (var rule in DiscoverExternalRules()) {
                    // Duplicate rule, e.g. one from Skyrim Data VM and the other from a mod folder
                    // Keep the more recently updated one, and discard the other.
                    var existingRule = Rules.FirstOrDefault(r => r.Uid == rule.Uid);
                    if (existingRule is not null) {
                        if (rule.Underlying.IsMoreRecentThan(existingRule.Underlying)) {
                            Rules.Remove(existingRule);
                            Rules.Add(rule);
                        }
                    } else {
                        Rules.Add(rule);
                    }
                    Debug.Assert(Rules.FirstOrDefault(r => r.Uid == rule.Uid) is not null);
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to discover rules: {ex.Message}");
            }

            // Reconcile the old packs with the newly discovered ones (all of which are primordial)
            // If an old pack is not present in the newly discovered packs, it is ephemeral and should be added back
            // If an old pack is present, then it was primordial before. Pick the more recently updated version
            foreach (var oldRule in oldRules) {
                var newRule = Rules.FirstOrDefault(rule => rule.Uid == oldRule.Uid);
                // Case 1: the pack does not already exist => ephemeral pack, add it back to the list
                if (newRule is null) {
                    var newConstruct = new RuleConstruct(oldRule, isPrimordial: false);
                    Rules.Add(newConstruct);
                    continue;
                }
                // Case 2: the pack exists => primordial pack, take the more recently updated version
                // Case 2.1: the pack was not changed since the last discovery => oldPack == newPack (no replacement needed)
                // Case 2.2: the pack was changed since the last discovery but not exported => oldPack more recent (replace)
                // Case 2.3: the pack was changed since the last discovery and exported => oldPack == newPack
                // Case 2.4: the pack was changed outside of the app => unspecified
                Debug.Assert(newRule.Primordial is not null);
                Debug.Assert(newRule.Primordial.LastUpdatedUtc == newRule.Underlying.LastUpdatedUtc);
                if (oldRule.IsMoreRecentThan(newRule.Primordial)) {
                    var newConstruct = new RuleConstruct(oldRule, isPrimordial: true);
                    Rules.Remove(newRule);
                    Rules.Add(newConstruct);
                }
            }
        }

        public void Add(RuleConstruct? rule)
        {
            var newRule = CreateNewRule(rule?.Name);
            Rules.Add(newRule);
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
            SaveAs(rule, rule.SourceFilePath
                ?? throw new InvalidOperationException("Cannot save a rule without a source file path."));
        }

        public void SaveAs(RuleConstruct rule, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) {
                return;
            }

            var normalizedPath = EnsureJsonExtension(filePath);
            WriteRuleToDisk(rule.Underlying, normalizedPath);

            var current = rule as RuleConstruct;
            if (current is not null) {
                var sourcePath = IsPathDiscoverable(normalizedPath) ? normalizedPath : current.SourceFilePath;
                Rules.Remove(current);
                Rules.Add(new RuleConstruct(current.Underlying, isPrimordial: true) {
                    SourceFilePath = sourcePath
                });
            }

            ResetRuleList();
        }

        private RuleConstruct CreateNewRule(string? baseName = null)
        {
            baseName = baseName is not null
                ? System.Text.RegularExpressions.Regex.Replace(baseName, @"\s*\(\d+\)$", string.Empty)
                : "New Rule";

            var regex = new System.Text.RegularExpressions.Regex(
                $@"^{System.Text.RegularExpressions.Regex.Escape(baseName)}\s\((\d+)\)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var hasBaseName = Rules.Any(existingRule =>
                existingRule.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase));

            var maxSuffix = Rules
                .Select(existingRule => regex.Match(existingRule.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .DefaultIfEmpty(hasBaseName ? 0 : -1)
                .Max();

            return new RuleConstruct(new Rule {
                Name = maxSuffix > -1 ? $"{baseName} ({maxSuffix + 1})" : baseName,
            });
        }

        private IEnumerable<RuleConstruct> DiscoverExternalRules()
        {
            foreach (var ruleFile in DirectoryIterator.EnumerateProjectFiles([
                         new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                         new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                     ], RulesDirectoryInfix, "*.json")) {
                var primordial = TryReadRule(ruleFile);
                if (primordial is null) {
                    Debug.WriteLine($"Failed to read rule file '{ruleFile.FullName}'.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(primordial.Name)) {
                    primordial.Name = Path.GetFileNameWithoutExtension(ruleFile.Name);
                }

                yield return new RuleConstruct(primordial, isPrimordial: true) {
                    SourceFilePath = ruleFile.FullName
                };
            }
        }

        private static Rule? TryReadRule(FileInfo ruleFile)
        {
            try {
                if (!ruleFile.Exists) {
                    return null;
                }

                var json = File.ReadAllText(ruleFile.FullName);
                return JsonSerializer.Deserialize<Rule>(json, JsonConfiguration.Configuration);
            } catch (Exception ex) when (ex is not JsonException) {
                Debug.WriteLine($"Failed to read rule file '{ruleFile.FullName}': {ex.Message}");
            } catch (JsonException ex) {
                Debug.WriteLine($"Failed to parse rule json '{ruleFile.FullName}': {ex.Message}");
            }

            return null;
        }

        private static void WriteRuleToDisk(Rule rule, string filePath)
        {
            var outputPath = EnsureJsonExtension(filePath);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory)) {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(rule, JsonConfiguration.Configuration);
            File.WriteAllText(outputPath, json);
        }

        private bool IsPathDiscoverable(string filePath)
        {
            return DirectoryIterator.EnumerateProjectFiles([
                    new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                    new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                ], RulesDirectoryInfix, "*.json")
                .Any(file => string.Equals(file.FullName, filePath, StringComparison.OrdinalIgnoreCase));
        }

        private static string EnsureJsonExtension(string filePath)
        {
            return filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? filePath
                : filePath + ".json";
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
