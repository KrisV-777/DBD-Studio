using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Component.Condition;

namespace DBDStudio.Tests;

public sealed class ConditionDefinitionTests
{
    [Fact]
    public void AllConditionTypes_HaveDefinitionsWithoutThrowing()
    {
        foreach (var type in Enum.GetValues<ConditionType>()) {
            var values = ConditionDefinition.GetValuesForType(type).ToArray();
            Assert.NotNull(values);
        }
    }

    [Fact]
    public void GetStageDone_HasQuestAndIntegerArguments()
    {
        var values = ConditionDefinition.GetValuesForType(ConditionType.GetStageDone).ToArray();

        Assert.Equal(2, values.Length);
        var formArg = Assert.IsType<ConditionValue.Form>(values[0]);
        Assert.Equal(FormType.Quest, formArg.FilteredFormType);
        Assert.IsType<ConditionValue.Integer>(values[1]);
    }
}
