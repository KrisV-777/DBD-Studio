using System.Text.Json;

namespace DBDStudio.Converter.Json
{
    public enum SerializationMode
    {
        /// <summary>
        /// Write the json for local use. All properties will be included.
        /// </summary>
        Local,
        /// <summary>
        /// Write the json for publishing. Some properties will be emitted.
        /// </summary>
        Publish
    }

    public static class JsonConfiguration
    {
        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> used for serialization and deserialization of JSON data.
        /// </summary>
        public static JsonSerializerOptions Configuration { get; } = CreateJsonSerializerOptions();

        /// <summary>
        /// Gets or sets the serialization mode, which determines how certain properties are handled during serialization and deserialization.
        /// </summary>
        public static SerializationMode Mode { get; set; } = SerializationMode.Local;

        private static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Strict) {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = true
            };

            options.Converters.Add(new ConditionJsonConverter());
            options.Converters.Add(new IConditionJsonConverter());
            options.Converters.Add(new FormRecordJsonConverter());
            options.Converters.Add(new TextureMappingJsonConverter());

            return options;
        }
    }
}
