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

        private static string? ReadStringProperty(
            ref Utf8JsonReader reader, string? propertyName, string targetName, bool noThrow = false)
        {
            if (!string.Equals(propertyName, targetName, StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String) {
                if (noThrow)
                    return null;
                throw new JsonException($"{targetName} must be a string.");
            }

            return reader.GetString();
        }

        public override TextureMapping Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException("TextureMapping entries must be objects with VanillaTexture and ReplacementTexture properties.");
            }

            string? vanillaTexture = null;
            string? replacementTexture = null;
            string? absolutePath = null;

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Invalid TextureMapping payload.");

                var propertyName = reader.GetString();
                if (!reader.Read())
                    throw new JsonException("Invalid TextureMapping payload.");

                if (ReadStringProperty(ref reader, propertyName, nameof(TextureMapping.VanillaTexture)) is string vt) {
                    vanillaTexture = vt;
                } else if (ReadStringProperty(ref reader, propertyName, nameof(TextureMapping.ReplacementTexture)) is string rt) {
                    replacementTexture = rt;
                } else if (ReadStringProperty(ref reader, propertyName, nameof(TextureMapping.AbsolutePath), true) is string ap) {
                    absolutePath = ap;
                } else {
                    reader.Skip();
                }
            }

            if (string.IsNullOrWhiteSpace(vanillaTexture))
                throw new JsonException("TextureMapping VanillaTexture is required.");
            if (string.IsNullOrWhiteSpace(replacementTexture))
                throw new JsonException("TextureMapping ReplacementTexture is required.");
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
                absolutePath = null;

            return new TextureMapping(vanillaTexture, replacementTexture, absolutePath);
        }
    }
}
