using DBDStudio.Core.Models;
using DBDStudio.Core.Models.Textures;

namespace DBDStudio.Core.Persistence
{
    public sealed class PersistenceSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;
        public Dictionary<string, object?> Items { get; set; } = [];
    }

    public sealed class ApplicationSettingsPersistenceState
    {
        public string WorkspaceFilePath { get; set; } = string.Empty;
        public string SkyrimDataFolder { get; set; } = string.Empty;
        public string ModsFolder { get; set; } = string.Empty;
        public string BodySlidePresetsFolder { get; set; } = string.Empty;
        public string RaceMenuPresetsFolder { get; set; } = string.Empty;
        public double BaseFontSize { get; set; } = 14;
        public string Theme { get; set; } = "System";
    }

    public sealed class TexturePackPersistenceState
    {
        public List<TexturePack> Packs { get; set; } = [];
    }

    public sealed class BodySlidePresetPersistenceState
    {
        public List<BodySlidePreset> Presets { get; set; } = [];
    }

    public sealed class RaceMenuPresetPersistenceState
    {
        public List<RaceMenuPreset> Presets { get; set; } = [];
    }

    public sealed class RulePersistenceState
    {
        public List<Rule> Rules { get; set; } = [];
    }
}
