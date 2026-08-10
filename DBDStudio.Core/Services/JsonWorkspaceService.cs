using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Core.Converter.Json;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Models.Textures;

namespace DBDStudio.Core.Services
{
    internal sealed class JsonSnapShot
    {
        public ApplicationSettings Settings { get; set; } = new();
        public IReadOnlyList<TexturePack> TexturePacks { get; set; } = [];

        // TODO: Other collections (BodySlidePresets, RaceMenuPresets, Rules) when implemented.
    }

    internal sealed class JsonWorkspaceServiceConverter : JsonConverter<JsonSnapShot>
    {
        public override JsonSnapShot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var settings = JsonSerializer.Deserialize<ApplicationSettings>(
                root.GetProperty(nameof(JsonSnapShot.Settings)).GetRawText(),
                options)
                ?? throw new JsonException("Missing settings.");

            var texPacks = JsonSerializer.Deserialize<IReadOnlyList<TexturePack>>(
                root.GetProperty(nameof(JsonSnapShot.TexturePacks)).GetRawText(), options)
                ?? throw new JsonException("Missing texture packs.");

            return new JsonSnapShot {
                Settings = settings,
                TexturePacks = texPacks
            };
        }

        public override void Write(Utf8JsonWriter writer, JsonSnapShot value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(JsonSnapShot.Settings));
            JsonSerializer.Serialize(writer, value.Settings, options);

            writer.WritePropertyName(nameof(JsonSnapShot.TexturePacks));
            JsonSerializer.Serialize(writer, value.TexturePacks, options);

            writer.WriteEndObject();
        }
    }

    public sealed class JsonWorkspaceService(
        ITexturePackService texturePackService,
        IBodySlideService bodySlideService,
        IRaceMenuPresetService raceMenuPresetService,
        IRuleService ruleService) : IWorkspaceService
    {
        private readonly ITexturePackService _texturePackService = texturePackService;
        private readonly IBodySlideService _bodySlideService = bodySlideService;
        private readonly IRaceMenuPresetService _raceMenuPresetService = raceMenuPresetService;
        private readonly IRuleService _ruleService = ruleService;
        private ApplicationSettings _settings = new();

        public ApplicationSettings Settings => _settings;

        public IReadOnlyList<TexturePack> TexturePacks
        {
            get => [.. _texturePackService.GetTexturePacks().Select(tp => tp.Underlying)];
            set => _texturePackService.ResetTextureList(value);
        }

        public IReadOnlyList<BodySlidePreset> BodySlidePresets => throw new NotImplementedException();

        public IReadOnlyList<RaceMenuPreset> RaceMenuPresets => throw new NotImplementedException();

        public IReadOnlyList<Rule> Rules => throw new NotImplementedException();

        public void Load()
        {
            var workspacePath = ResolveWorkspacePath();
            if (!File.Exists(workspacePath)) {
                return;
            }

            var json = File.ReadAllText(workspacePath);
            var config = JsonConfiguration.Configuration;
            var snapshot = JsonSerializer.Deserialize<JsonSnapShot>(json, config);

            if (snapshot is null) {
                Debug.WriteLine($"Failed to load workspace from '{workspacePath}': Deserialized snapshot is null.");
                return;
            }

            _settings = snapshot.Settings;
            TexturePacks = snapshot.TexturePacks;
            // TODO: Other collections (BodySlidePresets, RaceMenuPresets, Rules) should also be loaded here when implemented.
        }

        public void Save()
        {
            var workspacePath = ResolveWorkspacePath();
            JsonConfiguration.Mode = SerializationMode.Local;
            var config = JsonConfiguration.Configuration;
            var snapshot = new JsonSnapShot {
                Settings = _settings,
                TexturePacks = TexturePacks
            };
            var json = JsonSerializer.Serialize(snapshot, config);
            File.WriteAllText(workspacePath, json);
        }

        #region Private Methods

        private string ResolveWorkspacePath()
        {
            var configured = _settings.WorkspaceFilePath;
            Debug.Assert(!string.IsNullOrWhiteSpace(configured));

            return configured.EndsWith(".dbdproj", StringComparison.OrdinalIgnoreCase)
                ? configured
                : configured + ".dbdproj";
        }

        #endregion
    }
}
