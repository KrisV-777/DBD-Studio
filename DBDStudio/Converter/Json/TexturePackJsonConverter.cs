using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Models.Textures;

namespace DBDStudio.Converter.Json
{
    public sealed class TexturePackJsonConverter : JsonConverter<TexturePack?>
    {
        public override void Write(
            Utf8JsonWriter writer,
            TexturePack? value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is not null) {
                writer.WritePropertyName(nameof(TexturePack.Uid));
                JsonSerializer.Serialize(writer, value.Uid, options);

                writer.WriteString(nameof(TexturePack.Name), value.Name);
                writer.WriteString(nameof(TexturePack.Description), value.Description);
                writer.WriteBoolean(nameof(TexturePack.IsPrivate), value.IsPrivate);

                writer.WritePropertyName(nameof(TexturePack.LastUpdatedUtc));
                JsonSerializer.Serialize(writer, value.LastUpdatedUtc, options);

                writer.WritePropertyName(nameof(TexturePack.Mappings));
                JsonSerializer.Serialize(writer, value.Mappings, options);
            }

            writer.WriteEndObject();
        }

        public override TexturePack? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            if (root.GetPropertyCount() == 0) {
                return null;
            }

            var guid = root.GetProperty(nameof(TexturePack.Uid)).GetGuid();
            var name = root.GetProperty(nameof(TexturePack.Name)).GetString()
                ?? throw new JsonException("Missing name.");
            var description = root.GetProperty(nameof(TexturePack.Description)).GetString()
                ?? throw new JsonException("Missing description.");
            var isPrivate = root.GetProperty(nameof(TexturePack.IsPrivate)).GetBoolean();
            var lastUpdatedUtc = root.GetProperty(nameof(TexturePack.LastUpdatedUtc)).GetDateTimeOffset();
            var mappings = JsonSerializer.Deserialize<List<TextureMapping>>(
                root.GetProperty(nameof(TexturePack.Mappings)).GetRawText(),
                options);

            return new TexturePack(
                guid,
                name,
                description,
                isPrivate,
                lastUpdatedUtc,
                mappings ?? []);
        }
    }
}
