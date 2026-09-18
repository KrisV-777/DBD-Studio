using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Models.Mutagen;
using DBDStudio.Utility;
using DBDStudio.Utility.Persistence;

namespace DBDStudio.Tests;

public sealed class ModelAndUtilityTests
{
    [Fact]
    public void ApplicationSettings_SaveAndRestore_RoundTripsValues()
    {
        var settings = new ApplicationSettings {
            WorkspaceFilePath = "c:/tmp/workspace",
            SkyrimDataFolder = "data",
            ModsFolder = "mods",
            BodySlidePresetsFolder = "body",
            RaceMenuPresetsFolder = "race",
            BaseFontSize = 17,
            Theme = "Dark"
        };

        var state = Assert.IsType<ApplicationSettingsPersistenceState>(settings.SaveState());

        var restored = new ApplicationSettings();
        restored.RestoreState(state);

        Assert.Equal("c:/tmp/workspace", restored.WorkspaceFilePath);
        Assert.Equal("data", restored.SkyrimDataFolder);
        Assert.Equal("mods", restored.ModsFolder);
        Assert.Equal("body", restored.BodySlidePresetsFolder);
        Assert.Equal("race", restored.RaceMenuPresetsFolder);
        Assert.Equal(17, restored.BaseFontSize);
        Assert.Equal("Dark", restored.Theme);
    }

    [Fact]
    public void CollectionFilter_AppliesCaseInsensitiveFilter()
    {
        var target = new System.Collections.ObjectModel.ObservableCollection<string>();
        var source = new[] { "Alpha", "Beta", "Gamma" };

        CollectionFilter.ApplyTextFilter(target, source, "be", item => item);

        Assert.Single(target);
        Assert.Equal("Beta", target[0]);
    }

    [Fact]
    public void FormReference_ToStringAndValidity_WorkAsExpected()
    {
        var reference = new FormReference("Skyrim.esm", 0x14);

        Assert.Equal("Skyrim.esm|0x000014", reference.ToString());
        Assert.True(reference.MaybeValid());
        Assert.False(new FormReference("", 0).MaybeValid());
    }

    [Fact]
    public void FormRecord_MatchQuery_MatchesHexAndNameAndEditorId()
    {
        var record = new FormRecord {
            Name = "Some NPC",
            EditorId = "NPC_Test",
            Plugin = "Skyrim.esm",
            FormId = 0xABCD,
            RecordType = "NPC_"
        };

        Assert.True(record.MatchQuery("0xABCD"));
        Assert.True(record.MatchQuery("some"));
        Assert.True(record.MatchQuery("npc_test"));
        Assert.False(record.MatchQuery("does-not-match"));
    }

    [Fact]
    public void Rule_CopyAndImport_PreservePublishedPath()
    {
        var source = new Rule { Name = "A", LastPublishedPath = "C:/tmp/rule.json" };
        var copy = (Rule)typeof(Rule)
            .GetMethod("Copy", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(source, null)!;

        Assert.Equal(source.LastPublishedPath, copy.LastPublishedPath);

        var target = new Rule { Name = "B" };
        typeof(Rule)
            .GetMethod("Import", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(target, new object[] { source });

        Assert.Equal("C:/tmp/rule.json", target.LastPublishedPath);
    }
}
