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

            writer.WriteStartObject();

            writer.WriteString("Type", value.ConditionType.ToString());
            writer.WriteString("Operator", value.OperatorSymbol);
            writer.WriteStartArray("Arguments");
            foreach (var argument in value.Values) {
                switch (argument) {
                case ConditionValue.String str:
                    writer.WriteStringValue(str.Value);
                    break;
                case ConditionValue.Integer i:
                    writer.WriteNumberValue(i.Value);
                    break;
                case ConditionValue.Float f:
                    writer.WriteNumberValue(f.Value);
                    break;
                case ConditionValue.Boolean b:
                    writer.WriteBooleanValue(b.Value);
                    break;
                case ConditionValue.Sex s:
                    writer.WriteStringValue(s.SelectedSex);
                    break;
                case ConditionValue.Form b:
                    writer.WriteStringValue(b.Value?.FormReference.ToString() ?? string.Empty);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported argument type: {argument.GetType().FullName}");
                }
            }
            writer.WriteEndArray();
            writer.WriteString("Conjunction", value.ConjunctionLabel);

            writer.WriteEndObject();
        }
    }
}
