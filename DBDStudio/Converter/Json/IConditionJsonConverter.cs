using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Component.Condition;

namespace DBDStudio.Converter.Json
{
    public sealed class IConditionJsonConverter : JsonConverter<ICondition>
    {
        public override ICondition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<Condition>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, ICondition value, JsonSerializerOptions options)
        {
            if (value is not Condition condition) {
                throw new NotSupportedException($"Unsupported condition type: {value.GetType().FullName}");
            }

            JsonSerializer.Serialize(writer, condition, options);
        }
    }
}
