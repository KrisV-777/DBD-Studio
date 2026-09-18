using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Models.Component.Condition;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Tests;

public sealed class ConditionValueTests
{
    [Fact]
    public void DeepClone_StringValue_CopiesValue()
    {
        var source = new ConditionValue.String { Value = "abc" };
        var clone = Assert.IsType<ConditionValue.String>(source.DeepClone());

        Assert.Equal("abc", clone.Value);
        Assert.False(ReferenceEquals(source, clone));
    }

    [Fact]
    public void DeepClone_SexValue_CopiesValue()
    {
        var source = new ConditionValue.Sex { Value = true };
        var clone = Assert.IsType<ConditionValue.Sex>(source.DeepClone());

        Assert.True(clone.Value);
        Assert.Equal("Male", clone.SelectedSex);
    }

    [Fact]
    public void DeepClone_FormValue_CopiesTypeAndRecord()
    {
        var record = new FormRecord { Plugin = "Skyrim.esm", FormId = 0x99, Name = "A", RecordType = "NPC_" };
        var source = new ConditionValue.Form(FormType.NPC) { Value = record };

        var clone = Assert.IsType<ConditionValue.Form>(source.DeepClone());

        Assert.Equal(FormType.NPC, clone.FilteredFormType);
        Assert.Equal(record, clone.Value);
        Assert.False(ReferenceEquals(source, clone));
    }

    [Fact]
    public void SexSelectedSex_SetsValue()
    {
        var sex = new ConditionValue.Sex();

        sex.SelectedSex = "Male";
        Assert.True(sex.Value);

        sex.SelectedSex = "Female";
        Assert.False(sex.Value);
    }
}
