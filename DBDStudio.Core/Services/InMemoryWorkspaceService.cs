using System.Text.Json;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services
{
    public sealed class InMemoryWorkspaceService : IWorkspaceService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() 
        {
            WriteIndented = true
        };

        private readonly string _defaultWorkspacePath;

        public InMemoryWorkspaceService()
        {
            _defaultWorkspacePath = BuildDefaultWorkspacePath();

            Current = new Workspace();
            Current.Settings.WorkspaceFilePath = _defaultWorkspacePath;
        }

        public Workspace Current { get; }

        public void Load()
        {
            var workspacePath = ResolveWorkspacePath();
            if (!File.Exists(workspacePath)) {
                return;
            }

            var json = File.ReadAllText(workspacePath);
            var snapshot = JsonSerializer.Deserialize<WorkspaceSnapshot>(json, JsonOptions);
            if (snapshot is null) {
                return;
            }

            ApplySnapshot(Current, snapshot);
            Current.Settings.WorkspaceFilePath = workspacePath;
        }

        public void Save()
        {
            var workspacePath = ResolveWorkspacePath();
            var directory = Path.GetDirectoryName(workspacePath);
            if (!string.IsNullOrWhiteSpace(directory)) {
                Directory.CreateDirectory(directory);
            }

            Current.Settings.WorkspaceFilePath = workspacePath;
            var snapshot = WorkspaceSnapshot.From(Current);
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            File.WriteAllText(workspacePath, json);
        }

        private string ResolveWorkspacePath()
        {
            var configured = Current.Settings.WorkspaceFilePath;
            if (string.IsNullOrWhiteSpace(configured)) {
                return _defaultWorkspacePath;
            }

            return configured.EndsWith(".dbdproj", StringComparison.OrdinalIgnoreCase)
                ? configured
                : configured + ".dbdproj";
        }

        private static string BuildDefaultWorkspacePath()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataFolder, "DBDStudio", "workspace.dbdproj");
        }

        private static void ApplySnapshot(Workspace target, WorkspaceSnapshot snapshot)
        {
            target.Settings.WorkspaceFilePath = snapshot.Settings.WorkspaceFilePath;
            target.Settings.SkyrimDataFolder = snapshot.Settings.SkyrimDataFolder;
            target.Settings.ModsFolder = snapshot.Settings.ModsFolder;
            target.Settings.BodySlidePresetsFolder = snapshot.Settings.BodySlidePresetsFolder;
            target.Settings.RaceMenuPresetsFolder = snapshot.Settings.RaceMenuPresetsFolder;
            target.Settings.BaseFontSize = snapshot.Settings.BaseFontSize;
            target.Settings.Theme = snapshot.Settings.Theme;

            return;
            
            // target.TexturePacks.Clear();
            // foreach (var texturePackSnapshot in snapshot.TexturePacks)
            // {
            //     var pack = new TexturePack(guid: texturePackSnapshot.Uid)
            //     {
            //         Name = texturePackSnapshot.Name,
            //         Description = texturePackSnapshot.Description,
            //         Visibility = texturePackSnapshot.Visibility,
            //         // LastUpdatedUtc = texturePackSnapshot.LastUpdatedUtc
            //     };

            //     foreach (var mappingSnapshot in texturePackSnapshot.Mappings)
            //     {
            //         pack.Mappings.Add(new TextureMapping
            //         {
            //             VanillaTexture = mappingSnapshot.VanillaTexture,
            //             ReplacementTexture = mappingSnapshot.ReplacementTexture,
            //             SourcePath = mappingSnapshot.SourcePath
            //         });
            //     }

            //     target.TexturePacks.Add(pack);
            // }

            // target.Rules.Clear();
            // foreach (var ruleSnapshot in snapshot.Rules)
            // {
            //     var rule = new Rule
            //     {
            //         Name = ruleSnapshot.Name,
            //         FileName = ruleSnapshot.FileName
            //     };
            //     foreach (var candidate in ruleSnapshot.TextureCandidates) {
            //         rule.TextureCandidates.Add(candidate);
            //     }

            //     foreach (var candidate in ruleSnapshot.BodySlideCandidates) {
            //         rule.BodySlideCandidates.Add(candidate);
            //     }

            //     foreach (var candidate in ruleSnapshot.RaceMenuCandidates) {
            //         rule.RaceMenuCandidates.Add(candidate);
            //     }

            //     foreach (var conditionSnapshot in ruleSnapshot.Conditions)
            //     {
            //         rule.Conditions.Add(new Condition
            //         {
            //             Type = conditionSnapshot.Type,
            //             Operator = conditionSnapshot.Operator,
            //             Value = conditionSnapshot.Value,
            //             Group = conditionSnapshot.Group
            //         });
            //     }

            //     target.Rules.Add(rule);
            // }
        }

        private sealed class WorkspaceSnapshot
        {
            public SettingsSnapshot Settings { get; init; } = new();
            public List<TexturePackSnapshot> TexturePacks { get; init; } = [];
            public List<RuleSnapshot> Rules { get; init; } = [];

            public static WorkspaceSnapshot From(Workspace workspace)
            {
                return new WorkspaceSnapshot
                {
                    Settings = new SettingsSnapshot
                    {
                        WorkspaceFilePath = workspace.Settings.WorkspaceFilePath,
                        SkyrimDataFolder = workspace.Settings.SkyrimDataFolder,
                        ModsFolder = workspace.Settings.ModsFolder,
                        BodySlidePresetsFolder = workspace.Settings.BodySlidePresetsFolder,
                        RaceMenuPresetsFolder = workspace.Settings.RaceMenuPresetsFolder,
                        BaseFontSize = workspace.Settings.BaseFontSize,
                        Theme = workspace.Settings.Theme,
                    },
                    TexturePacks = workspace.TexturePacks.Select(pack => new TexturePackSnapshot
                    {
                        Uid = pack.Uid,
                        Name = pack.Name,
                        Description = pack.Description,
                        // Visibility = pack.Visibility,
                        // LastUpdatedUtc = pack.LastUpdatedUtc,
                        Mappings = pack.Mappings.Select(mapping => new TextureMappingSnapshot
                        {
                            VanillaTexture = mapping.VanillaTexture,
                            ReplacementTexture = mapping.ReplacementTexture,
                            SourcePath = default
                        }).ToList()
                    }).ToList(),
                    Rules = workspace.Rules.Select(rule => new RuleSnapshot
                    {
                        Name = rule.Name,
                        FileName = rule.FileName,
                        TextureCandidates = rule.TextureCandidates.ToList(),
                        BodySlideCandidates = rule.BodySlideCandidates.ToList(),
                        RaceMenuCandidates = rule.RaceMenuCandidates.ToList(),
                        Conditions = rule.Conditions.Select(condition => new ConditionSnapshot
                        {
                            Type = condition.Type,
                            Operator = condition.Operator,
                            Value = condition.Value,
                            Group = condition.Group
                        }).ToList()
                    }).ToList()
                };
            }
        }

        private sealed class TexturePackSnapshot
        {
            public Guid Uid { get; init; } = Guid.Empty;
            public string Name { get; init; } = string.Empty;
            public string Description { get; init; } = string.Empty;
            // public TexturePackVisibility Visibility { get; init; }
            // public DateTimeOffset LastUpdatedUtc { get; init; } = DateTimeOffset.MinValue;
            public List<TextureMappingSnapshot> Mappings { get; init; } = [];
        }

        private sealed class TextureMappingSnapshot
        {
            public string VanillaTexture { get; init; } = string.Empty;
            public string ReplacementTexture { get; init; } = string.Empty;
            public string? SourcePath { get; init; } = string.Empty;
        }

        private sealed class SettingsSnapshot
        {
            public string WorkspaceFilePath { get; init; } = string.Empty;
            public string SkyrimDataFolder { get; init; } = string.Empty;
            public string ModsFolder { get; init; } = string.Empty;
            public string BodySlidePresetsFolder { get; init; } = string.Empty;
            public string RaceMenuPresetsFolder { get; init; } = string.Empty;
            public double BaseFontSize { get; init; } = 14;
            public string Theme { get; init; } = "System";
        }

        private sealed class RuleSnapshot
        {
            public string Name { get; init; } = string.Empty;
            public string FileName { get; init; } = string.Empty;
            public List<string> TextureCandidates { get; init; } = [];
            public List<string> BodySlideCandidates { get; init; } = [];
            public List<string> RaceMenuCandidates { get; init; } = [];
            public List<ConditionSnapshot> Conditions { get; init; } = [];
        }

        private sealed class ConditionSnapshot
        {
            public string Type { get; init; } = string.Empty;
            public string Operator { get; init; } = "==";
            public string Value { get; init; } = string.Empty;
            public int Group { get; init; }
        }
    }
}
