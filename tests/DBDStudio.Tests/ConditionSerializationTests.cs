using System.Text.Json;
using DBDStudio.Converter.Json;
using DBDStudio.Models.Component;
using DBDStudio.Models.Component.Condition;

namespace DBDStudio.Tests;

public sealed class ConditionSerializationTests
{
    [Fact]
    public void Condition_RoundTrip_DoesNotDuplicateArguments()
    {
        var options = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Local);
        var condition = new Condition();

        var json = JsonSerializer.Serialize(condition, options);
        var roundTrip = JsonSerializer.Deserialize<Condition>(json, options);

        Assert.NotNull(roundTrip);
        Assert.Equal(condition.Arguments.Count, roundTrip!.Arguments.Count);
    }

    [Fact]
    public void Rule_RoundTrip_PreservesSingleConditionAndArguments()
    {
        var options = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Local);
        var condition = new Condition();
        var rule = new Rule { Name = "Roundtrip" };
        rule.Conditions.Add(condition);

        var json = JsonSerializer.Serialize(rule, options);
        var roundTrip = JsonSerializer.Deserialize<Rule>(json, options);

        Assert.NotNull(roundTrip);
        Assert.Single(roundTrip!.Conditions);

        var restoredCondition = Assert.IsType<Condition>(roundTrip.Conditions[0]);
        Assert.Equal(condition.Arguments.Count, restoredCondition.Arguments.Count);
    }
}
