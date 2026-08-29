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
        /// Gets or sets the serialization mode, which determines how certain properties are handled during serialization and deserialization.
        /// </summary>
        public static SerializationMode Mode { get; private set; } = SerializationMode.Local;

        /// <summary>
        /// Builds the JsonSerializerOptions configured for the specified serialization mode.
        /// </summary>
        /// <param name="mode">The serialization mode to configure the JsonSerializerOptions for.</param>
        /// <returns>The configured JsonSerializerOptions instance.</returns>
        public static JsonSerializerOptions BuildJsonConfiguration(SerializationMode mode)
        {
            Mode = mode;

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Strict) {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = true
            };

            if (Mode == SerializationMode.Publish) {
                options.Converters.Add(new PublishedConditionJsonConverter());
                options.Converters.Add(new PublishedFormRecordJsonConverter());
            }
            options.Converters.Add(new IConditionJsonConverter());
            options.Converters.Add(new TextureMappingJsonConverter());

            return options;
        }
    }
}
