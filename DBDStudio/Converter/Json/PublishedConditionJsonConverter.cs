using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Component.Condition;

namespace DBDStudio.Converter.Json
{
    public sealed class PublishedConditionJsonConverter : JsonConverter<Condition>
    {
        public override Condition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("PublishedConditionJsonConverter does not support reading JSON.");

        public override void Write(Utf8JsonWriter writer, Condition value, JsonSerializerOptions options)
        {
            Debug.Assert(JsonConfiguration.Mode == SerializationMode.Publish);

            var argumentStr = string.Join(" ", value.Arguments.Select(v => v switch {
                ConditionValue.String str => str.Value,
                ConditionValue.Integer i => i.Value.ToString(),
                ConditionValue.Float f => f.Value.ToString(),
                ConditionValue.Boolean b => b.Value.ToString(),
                ConditionValue.Sex s => s.SelectedSex,
                ConditionValue.Form f => f.Value?.FormReference.ToString() ?? "null",
                _ => throw new NotImplementedException(),
            }));
            writer.WriteStringValue($"{value.ConditionType} {argumentStr} {value.OperatorSymbol} {value.Comparator} {value.ConjunctionLabel}");
        }
    }
}
