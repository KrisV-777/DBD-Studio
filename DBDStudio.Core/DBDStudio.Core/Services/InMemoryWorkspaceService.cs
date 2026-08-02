using System.Text.Json;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

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
        Current = CreateDefaultWorkspace(_defaultWorkspacePath);
    }

    public Workspace Current { get; }

    public void Load()
    {
        var workspacePath = ResolveWorkspacePath();
        if (!File.Exists(workspacePath))
            return;

        var json = File.ReadAllText(workspacePath);
        var snapshot = JsonSerializer.Deserialize<WorkspaceSnapshot>(json, JsonOptions);
        if (snapshot is null)
            return;

        ApplySnapshot(Current, snapshot);
        Current.Settings.WorkspaceFilePath = workspacePath;
    }

    public void Save()
    {
        var workspacePath = ResolveWorkspacePath();
        var directory = Path.GetDirectoryName(workspacePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        Current.Settings.WorkspaceFilePath = workspacePath;
        var snapshot = WorkspaceSnapshot.From(Current);
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        File.WriteAllText(workspacePath, json);
    }

    private string ResolveWorkspacePath()
    {
        var configured = Current.Settings.WorkspaceFilePath;
        if (string.IsNullOrWhiteSpace(configured))
            return _defaultWorkspacePath;

        return configured.EndsWith(".dbdproj", StringComparison.OrdinalIgnoreCase)
            ? configured
            : configured + ".dbdproj";
    }

    private static string BuildDefaultWorkspacePath()
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataFolder, "DBDStudio", "workspace.dbdproj");
    }

    private static Workspace CreateDefaultWorkspace(string workspacePath)
    {
        var workspace = new Workspace();
        workspace.Settings.WorkspaceFilePath = workspacePath;

        workspace.TexturePacks.Add(new TexturePack { Name = "Fair Skin", Description = "A broad, bright skin option for female NPCs.", Visibility = TexturePackVisibility.Public, RandomPool = true });
        workspace.TexturePacks.Add(new TexturePack { Name = "Bijin", Description = "High-contrast character textures.", Visibility = TexturePackVisibility.Public, RandomPool = false });
        workspace.TexturePacks.Add(new TexturePack { Name = "Tempered", Description = "Balanced textures for a clean look.", Visibility = TexturePackVisibility.Private, RandomPool = false });
        workspace.TexturePacks.Add(new TexturePack { Name = "Player HD", Description = "A high-detail player texture set.", Visibility = TexturePackVisibility.Public, RandomPool = true });

        workspace.BodySlidePresets.Add(new BodySlidePreset { Preset = "CBBE Curvy", SourceXml = "CBBE.xml" });
        workspace.BodySlidePresets.Add(new BodySlidePreset { Preset = "BHUNP Slim", SourceXml = "BHUNP.xml" });
        workspace.BodySlidePresets.Add(new BodySlidePreset { Preset = "UUNP Special", SourceXml = "UUNP.xml" });

        workspace.RaceMenuPresets.Add(new RaceMenuPreset
        {
            Name = "LydiaPreset",
            JsSlotFile = "LydiaPreset.jslot",
            Sex = "Female",
            NifFile = "LydiaPreset.nif",
            DdsFile = "LydiaPreset.dds"
        });
        workspace.RaceMenuPresets.Add(new RaceMenuPreset
        {
            Name = "WarriorMale",
            JsSlotFile = "WarriorMale.jslot",
            Sex = "Male",
            NifFile = "WarriorMale.nif"
        });
        workspace.RaceMenuPresets.Add(new RaceMenuPreset
        {
            Name = "CustomFemale",
            JsSlotFile = "CustomFemale.jslot",
            Sex = "Female"
        });

        workspace.Rules.Add(new Rule
        {
            Name = "Bandits",
            FileName = "Bandits.yaml",
            TexturePack = "Tempered",
            BodySlidePreset = "BHUNP Slim",
            RaceMenuPreset = "LydiaPreset",
            Conditions =
            {
                new Condition { Type = "Faction", Operator = "==", Value = "Bandits", Group = 0 },
                new Condition { Type = "Sex", Operator = "==", Value = "Female", Group = 1 }
            }
        });

        workspace.Rules.Add(new Rule
        {
            Name = "Companions",
            FileName = "Companions.yaml",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            RaceMenuPreset = "WarriorMale",
            Conditions =
            {
                new Condition { Type = "Faction", Operator = "==", Value = "Companions", Group = 0 },
                new Condition { Type = "Race", Operator = "==", Value = "Nord", Group = 1 }
            }
        });

        workspace.Rules.Add(new Rule
        {
            Name = "Unique NPC",
            FileName = "Unique NPC.yaml",
            TexturePack = "Player HD",
            BodySlidePreset = "UUNP Special",
            Conditions =
            {
                new Condition { Type = "ReferenceID", Operator = "==", Value = "0x12345", Group = 0 }
            }
        });

        workspace.Rules.Add(new Rule
        {
            Name = "Fallback",
            FileName = "Fallback.yaml",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            Conditions =
            {
                new Condition { Type = "Sex", Operator = "==", Value = "Female", Group = 0 }
            }
        });

        return workspace;
    }

    private static void ApplySnapshot(Workspace target, WorkspaceSnapshot snapshot)
    {
        target.Settings.WorkspaceFilePath = snapshot.Settings.WorkspaceFilePath;
        target.Settings.SkyrimDataFolder = snapshot.Settings.SkyrimDataFolder;
        target.Settings.ModsFolder = snapshot.Settings.ModsFolder;
        target.Settings.BodySlidePresetsFolder = snapshot.Settings.BodySlidePresetsFolder;
        target.Settings.RaceMenuPresetsFolder = snapshot.Settings.RaceMenuPresetsFolder;

        target.TexturePacks.Clear();
        foreach (var packSnapshot in snapshot.TexturePacks)
        {
            var pack = new TexturePack
            {
                Name = packSnapshot.Name,
                Description = packSnapshot.Description,
                Visibility = packSnapshot.Visibility,
                RandomPool = packSnapshot.RandomPool
            };
            foreach (var mappingSnapshot in packSnapshot.Mappings)
            {
                pack.Mappings.Add(new TextureMapping
                {
                    VanillaTexture = mappingSnapshot.VanillaTexture,
                    ReplacementTexture = mappingSnapshot.ReplacementTexture,
                    SourcePath = mappingSnapshot.SourcePath
                });
            }

            target.TexturePacks.Add(pack);
        }

        target.BodySlidePresets.Clear();
        foreach (var preset in snapshot.BodySlidePresets)
            target.BodySlidePresets.Add(new BodySlidePreset { Preset = preset.Preset, SourceXml = preset.SourceXml });

        target.RaceMenuPresets.Clear();
        foreach (var preset in snapshot.RaceMenuPresets)
        {
            target.RaceMenuPresets.Add(new RaceMenuPreset
            {
                Name = preset.Name,
                JsSlotFile = preset.JsSlotFile,
                Sex = preset.Sex,
                NifFile = preset.NifFile,
                DdsFile = preset.DdsFile
            });
        }

        target.Rules.Clear();
        foreach (var ruleSnapshot in snapshot.Rules)
        {
            var rule = new Rule
            {
                Name = ruleSnapshot.Name,
                FileName = ruleSnapshot.FileName
            };
            foreach (var candidate in ruleSnapshot.TextureCandidates)
                rule.TextureCandidates.Add(candidate);
            foreach (var candidate in ruleSnapshot.BodySlideCandidates)
                rule.BodySlideCandidates.Add(candidate);
            foreach (var candidate in ruleSnapshot.RaceMenuCandidates)
                rule.RaceMenuCandidates.Add(candidate);
            foreach (var conditionSnapshot in ruleSnapshot.Conditions)
            {
                rule.Conditions.Add(new Condition
                {
                    Type = conditionSnapshot.Type,
                    Operator = conditionSnapshot.Operator,
                    Value = conditionSnapshot.Value,
                    Group = conditionSnapshot.Group
                });
            }

            target.Rules.Add(rule);
        }
    }

    private sealed class WorkspaceSnapshot
    {
        public SettingsSnapshot Settings { get; init; } = new();
        public List<TexturePackSnapshot> TexturePacks { get; init; } = [];
        public List<BodySlidePresetSnapshot> BodySlidePresets { get; init; } = [];
        public List<RaceMenuPresetSnapshot> RaceMenuPresets { get; init; } = [];
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
                    RaceMenuPresetsFolder = workspace.Settings.RaceMenuPresetsFolder
                },
                TexturePacks = workspace.TexturePacks.Select(pack => new TexturePackSnapshot
                {
                    Name = pack.Name,
                    Description = pack.Description,
                    Visibility = pack.Visibility,
                    RandomPool = pack.RandomPool,
                    Mappings = pack.Mappings.Select(mapping => new TextureMappingSnapshot
                    {
                        VanillaTexture = mapping.VanillaTexture,
                        ReplacementTexture = mapping.ReplacementTexture,
                        SourcePath = mapping.SourcePath
                    }).ToList()
                }).ToList(),
                BodySlidePresets = workspace.BodySlidePresets.Select(preset => new BodySlidePresetSnapshot
                {
                    Preset = preset.Preset,
                    SourceXml = preset.SourceXml
                }).ToList(),
                RaceMenuPresets = workspace.RaceMenuPresets.Select(preset => new RaceMenuPresetSnapshot
                {
                    Name = preset.Name,
                    JsSlotFile = preset.JsSlotFile,
                    Sex = preset.Sex,
                    NifFile = preset.NifFile,
                    DdsFile = preset.DdsFile
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

    private sealed class SettingsSnapshot
    {
        public string WorkspaceFilePath { get; init; } = string.Empty;
        public string SkyrimDataFolder { get; init; } = string.Empty;
        public string ModsFolder { get; init; } = string.Empty;
        public string BodySlidePresetsFolder { get; init; } = string.Empty;
        public string RaceMenuPresetsFolder { get; init; } = string.Empty;
    }

    private sealed class TexturePackSnapshot
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public TexturePackVisibility Visibility { get; init; }
        public bool RandomPool { get; init; }
        public List<TextureMappingSnapshot> Mappings { get; init; } = [];
    }

    private sealed class TextureMappingSnapshot
    {
        public string VanillaTexture { get; init; } = string.Empty;
        public string ReplacementTexture { get; init; } = string.Empty;
        public string SourcePath { get; init; } = string.Empty;
    }

    private sealed class BodySlidePresetSnapshot
    {
        public string Preset { get; init; } = string.Empty;
        public string SourceXml { get; init; } = string.Empty;
    }

    private sealed class RaceMenuPresetSnapshot
    {
        public string Name { get; init; } = string.Empty;
        public string JsSlotFile { get; init; } = string.Empty;
        public string Sex { get; init; } = "Male";
        public string? NifFile { get; init; }
        public string? DdsFile { get; init; }
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
