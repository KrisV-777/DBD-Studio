using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Component.Condition;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Tests;

public sealed class ConditionBehaviorTests
{
    [Fact]
    public void Constructor_InitializesArgumentsForDefaultType()
    {
        var condition = new Condition();

        Assert.Equal(ConditionType.GetIsReference, condition.ConditionType);
        Assert.Single(condition.Arguments);
        var form = Assert.IsType<ConditionValue.Form>(condition.Arguments[0]);
        Assert.Equal(FormType.ActorRef, form.FilteredFormType);
    }

    [Fact]
    public void ConditionTypeChange_PreservesCompatibleValues()
    {
        var condition = new Condition {
            ConditionType = ConditionType.GetStage
        };

        var stageArg = Assert.IsType<ConditionValue.Form>(condition.Arguments[0]);
        stageArg.Value = new FormRecord { Plugin = "Skyrim.esm", FormId = 0x12, Name = "QuestRef", RecordType = "QUST" };

        condition.ConditionType = ConditionType.GetStageDone;

        Assert.Equal(2, condition.Arguments.Count);
        var firstArg = Assert.IsType<ConditionValue.Form>(condition.Arguments[0]);
        Assert.Equal(stageArg.Value, firstArg.Value);
        Assert.IsType<ConditionValue.Integer>(condition.Arguments[1]);
    }

    [Fact]
    public void OperatorSymbol_RoundTripsToOperator()
    {
        var condition = new Condition();

        condition.OperatorSymbol = "!=";

        Assert.Equal(Operator.NotEquals, condition.Operator);
        Assert.Equal("!=", condition.OperatorSymbol);
    }

    [Fact]
    public void ConjunctionLabel_RoundTripsToConjunction()
    {
        var condition = new Condition();

        condition.ConjunctionLabel = "OR";

        Assert.Equal(Conjunction.Or, condition.Conjunction);
        Assert.Equal("OR", condition.ConjunctionLabel);
    }

    [Fact]
    public void Copy_ClonesArgumentsWithoutSharingInstances()
    {
        var condition = new Condition {
            ConditionType = ConditionType.GetActorValue
        };
        var arg = Assert.IsType<ConditionValue.String>(condition.Arguments[0]);
        arg.Value = "Health";

        var copy = Assert.IsType<Condition>(condition.Copy());
        var copiedArg = Assert.IsType<ConditionValue.String>(copy.Arguments[0]);

        Assert.Equal("Health", copiedArg.Value);
        Assert.False(ReferenceEquals(condition.Arguments[0], copy.Arguments[0]));
    }
}
