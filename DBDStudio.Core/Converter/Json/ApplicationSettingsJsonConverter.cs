using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Converter.Json
{
    public sealed class ApplicationSettingsJsonConverter : JsonConverter<ApplicationSettings>
    {
        public override void Write(
            Utf8JsonWriter writer,
            ApplicationSettings value,
            JsonSerializerOptions options)
        {
            if (JsonConfiguration.Mode == SerializationMode.Publish) {
                throw new InvalidOperationException("ApplicationSettings should not be serialized in publish mode.");
            }

            writer.WriteStartObject();

            writer.WriteString(nameof(ApplicationSettings.WorkspaceFilePath), value.WorkspaceFilePath);
            writer.WriteString(nameof(ApplicationSettings.SkyrimDataFolder), value.SkyrimDataFolder);
            writer.WriteString(nameof(ApplicationSettings.ModsFolder), value.ModsFolder);
            writer.WriteString(nameof(ApplicationSettings.BodySlidePresetsFolder), value.BodySlidePresetsFolder);
            writer.WriteString(nameof(ApplicationSettings.RaceMenuPresetsFolder), value.RaceMenuPresetsFolder);
            writer.WriteNumber(nameof(ApplicationSettings.BaseFontSize), value.BaseFontSize);
            writer.WriteString(nameof(ApplicationSettings.Theme), value.Theme);

            writer.WriteEndObject();
        }

        public override ApplicationSettings Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var workspaceFilePath =
                root.GetProperty(nameof(ApplicationSettings.WorkspaceFilePath)).GetString()
                ?? throw new JsonException("Missing workspaceFilePath.");

            var skyrimDataFolder =
                root.GetProperty(nameof(ApplicationSettings.SkyrimDataFolder)).GetString()
                ?? throw new JsonException("Missing skyrimDataFolder.");

            var modsFolder =
                root.GetProperty(nameof(ApplicationSettings.ModsFolder)).GetString()
                ?? throw new JsonException("Missing modsFolder.");

            var bodySlidePresetsFolder =
                root.GetProperty(nameof(ApplicationSettings.BodySlidePresetsFolder)).GetString()
                ?? throw new JsonException("Missing bodySlidePresetsFolder.");

            var raceMenuPresetsFolder =
                root.GetProperty(nameof(ApplicationSettings.RaceMenuPresetsFolder)).GetString()
                ?? throw new JsonException("Missing raceMenuPresetsFolder.");

            var baseFontSize =
                root.GetProperty(nameof(ApplicationSettings.BaseFontSize)).GetDouble();

            var theme =
                root.GetProperty(nameof(ApplicationSettings.Theme)).GetString()
                ?? throw new JsonException("Missing theme.");

            return new ApplicationSettings {
                WorkspaceFilePath = workspaceFilePath,
                SkyrimDataFolder = skyrimDataFolder,
                ModsFolder = modsFolder,
                BodySlidePresetsFolder = bodySlidePresetsFolder,
                RaceMenuPresetsFolder = raceMenuPresetsFolder,
                BaseFontSize = baseFontSize,
                Theme = theme
            };
        }
    }
}