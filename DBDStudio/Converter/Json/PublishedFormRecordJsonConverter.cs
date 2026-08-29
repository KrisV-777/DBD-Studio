using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Converter.Json
{
    public sealed class PublishedFormRecordJsonConverter : JsonConverter<FormRecord?>
    {
        public override FormRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("PublishedFormRecordJsonConverter does not support reading JSON.");

        public override void Write(Utf8JsonWriter writer, FormRecord? value, JsonSerializerOptions options)
        {
            Debug.Assert(JsonConfiguration.Mode == SerializationMode.Publish);
            if (value is null) {
                writer.WriteNullValue();
            } else {
                JsonSerializer.Serialize(writer, value.FormReference, options);
            }
        }
    }
}
