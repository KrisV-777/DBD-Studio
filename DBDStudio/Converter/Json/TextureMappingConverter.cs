using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Models.Component.Textures;

namespace DBDStudio.Converter.Json
{
    public sealed class TextureMappingJsonConverter : JsonConverter<TextureMapping>
    {
        public override void Write(Utf8JsonWriter writer, TextureMapping value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(nameof(TextureMapping.VanillaTexture), value.VanillaTexture);
            writer.WriteString(nameof(TextureMapping.ReplacementTexture), value.ReplacementTexture);

            if (JsonConfiguration.Mode == SerializationMode.Local) {
                writer.WriteString(nameof(TextureMapping.AbsolutePath), value.AbsolutePath);
            }

            writer.WriteEndObject();
        }

        public override TextureMapping Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var vanillaTexture =
                root.GetProperty(nameof(TextureMapping.VanillaTexture)).GetString()
                ?? throw new JsonException("Missing vanillaTexture.");

            var replacementTexture =
                root.GetProperty(nameof(TextureMapping.ReplacementTexture)).GetString()
                ?? throw new JsonException("Missing replacementTexture.");

            var absolutePath = root.GetProperty(nameof(TextureMapping.AbsolutePath)).GetString();
            if (!string.IsNullOrEmpty(absolutePath) && !File.Exists(absolutePath)) {
                absolutePath = null;
            }

            return new TextureMapping(vanillaTexture, replacementTexture, absolutePath);
        }
    }
}
