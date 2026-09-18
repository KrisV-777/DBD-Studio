using DBDStudio.Interfaces.Rules;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Models.Component.Condition;
using DBDStudio.Models.Mutagen;
using DBDStudio.Services;
using DBDStudio.ViewModels;

namespace DBDStudio.Tests;

public sealed class RulesViewModelTests
{
    [Fact]
    public void AddRuleCommand_AddsRule()
    {
        var vm = CreateViewModel();
        var before = vm.Rules.Count;

        vm.AddRuleCommand.Execute(null);

        Assert.Equal(before + 1, vm.Rules.Count);
    }

    [Fact]
    public void SaveRuleAs_PublishesSelectedRule()
    {
        using var temp = new TempDir();
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);

        var path = Path.Combine(temp.Path, "rule-out.json");
        vm.SaveRuleAs(path);

        Assert.NotNull(vm.SelectedRenderedRule);
        Assert.Equal(Path.GetFullPath(path), vm.SelectedRenderedRule!.Underlying.LastPublishedPath);
        Assert.True(File.Exists(Path.GetFullPath(path)));
    }

    [Fact]
    public void SaveAs_ReplaceFlow_KeepsSelectionOnNewInstance()
    {
        using var temp = new TempDir();
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);

        var before = vm.SelectedRenderedRule;
        vm.SaveRuleAs(Path.Combine(temp.Path, "replace.json"));
        var after = vm.SelectedRenderedRule;

        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.False(ReferenceEquals(before, after));
    }

    [Fact]
    public void RaceMenuWarning_RequiresReferenceOrNpcCondition()
    {
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);

        vm.SelectedRule!.RaceMenuCandidate = "some.jslot";

        Assert.Equal("RaceMenu assignments require GetIsReference or IsNPC conditions.", vm.RaceMenuAssignmentWarning);
    }

    [Fact]
    public void RaceMenuWarning_RequiresSexConditionForPlayer()
    {
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);

        var condition = new Condition { ConditionType = ConditionType.GetIsReference };
        var arg = Assert.IsType<ConditionValue.Form>(condition.Arguments[0]);
        arg.Value = new FormRecord { Plugin = "Skyrim.esm", FormId = 0x14, Name = "Player", RecordType = "REFR" };

        vm.SelectedRule!.Conditions.Add(condition);
        vm.SelectedRule.RaceMenuCandidate = "player.jslot";

        Assert.Equal("RaceMenu assignments need a IsSex condition to work with the player.", vm.RaceMenuAssignmentWarning);
    }

    [Fact]
    public void RaceMenuWarning_ClearsWhenSexConditionAdded()
    {
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);

        var referenceCondition = new Condition { ConditionType = ConditionType.GetIsReference };
        var refArg = Assert.IsType<ConditionValue.Form>(referenceCondition.Arguments[0]);
        refArg.Value = new FormRecord { Plugin = "Skyrim.esm", FormId = 0x14, Name = "Player", RecordType = "REFR" };
        vm.SelectedRule!.Conditions.Add(referenceCondition);

        var sexCondition = new Condition { ConditionType = ConditionType.GetIsSex };
        vm.SelectedRule.Conditions.Add(sexCondition);
        vm.SelectedRule.RaceMenuCandidate = "player.jslot";

        Assert.Equal(string.Empty, vm.RaceMenuAssignmentWarning);
    }

    [Fact]
    public void DeleteRuleCommand_DeletesEphemeralRule()
    {
        var vm = CreateViewModel();
        vm.AddRuleCommand.Execute(null);
        var before = vm.Rules.Count;

        vm.DeleteRuleCommand.Execute(null);

        Assert.Equal(before - 1, vm.Rules.Count);
    }

    private static RulesViewModel CreateViewModel()
    {
        var settings = new ApplicationSettings();
        var ruleService = new RuleService(settings);

        var textureService = new FakeTexturePackService();
        textureService.TexturePacks.Add(new TexturePackConstruct(new TexturePack { Name = "PackA" }));

        var bodyService = new FakeBodySlideService();
        bodyService.Presets.Add(new BodySlidePreset { Name = "PresetA", SourceXml = "x.xml" });

        var raceMenuService = new FakeRaceMenuPresetService();
        raceMenuService.Presets.Add(new RaceMenuPreset { Name = "R1", JslotFile = "r1.jslot" });

        var db = new FakeFormDatabase();

        return new RulesViewModel(ruleService, textureService, bodyService, raceMenuService, db);
    }
}
