using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Converter.Json
{
    public sealed class FormRecordJsonConverter : JsonConverter<FormRecord?>
    {
        public override void Write(Utf8JsonWriter writer, FormRecord? value, JsonSerializerOptions options)
        {
            if (value is null) {
                writer.WriteNullValue();
            } else if (JsonConfiguration.Mode == SerializationMode.Local) {
                JsonSerializer.Serialize(writer, value, options);
            } else {
                // Non-local: serialize only the FormReference
                JsonSerializer.Serialize(writer, value.FormReference, options);
            }
        }

        public override FormRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<FormRecord?>(ref reader, options);
    }
}
