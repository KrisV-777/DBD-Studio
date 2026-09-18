using System.Text.Json;
using DBDStudio.Converter.Json;
using DBDStudio.Models.Component.Condition;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Tests;

public sealed class PublishedJsonConverterTests
{
    [Fact]
    public void PublishMode_Condition_SerializesAsString()
    {
        var options = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Publish);
        var condition = new Condition {
            ConditionType = DBDStudio.Interfaces.Rules.ConditionType.GetActorValue,
            Comparator = 1,
            Conjunction = DBDStudio.Interfaces.Rules.Conjunction.And,
            Operator = DBDStudio.Interfaces.Rules.Operator.Equals
        };
        var arg = Assert.IsType<ConditionValue.String>(condition.Arguments[0]);
        arg.Value = "Health";

        var json = JsonSerializer.Serialize(condition, options);

        Assert.StartsWith("\"", json);
        Assert.Contains("GetActorValue", json);
        Assert.Contains("Health", json);
    }

    [Fact]
    public void PublishMode_FormRecord_SerializesAsFormReference()
    {
        var options = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Publish);
        var record = new FormRecord {
            Plugin = "Skyrim.esm",
            FormId = 0x14,
            Name = "PlayerRef",
            RecordType = "REFR"
        };

        var json = JsonSerializer.Serialize(record, options);

        Assert.Contains("Skyrim.esm", json);
        Assert.Contains("20", json); // 0x14 decimal serialization
    }
}
