using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Services;

namespace DBDStudio.Tests;

public sealed class PresetServiceRestoreTests
{
    [Fact]
    public void BodySlideService_RestoreState_OnlyKeepsExistingAndSorted()
    {
        using var temp = new TempDir();
        var existingFile = Path.Combine(temp.Path, "preset.xml");
        File.WriteAllText(existingFile, "<root />");

        var settings = new ApplicationSettings();
        var service = new BodySlideService(settings);
        service.RestoreState(new List<BodySlidePreset> {
            new BodySlidePreset { Name = "z", SourceXml = existingFile },
            new BodySlidePreset { Name = "", SourceXml = existingFile },
            new BodySlidePreset { Name = "a", SourceXml = Path.Combine(temp.Path, "missing.xml") }
        });

        var only = Assert.Single(service.Presets);
        Assert.Equal("z", only.Name);
    }

    [Fact]
    public void RaceMenuPresetService_RestoreState_OnlyKeepsExistingAndSorted()
    {
        using var temp = new TempDir();
        var existingFile = Path.Combine(temp.Path, "preset.jslot");
        File.WriteAllText(existingFile, "{}");

        var settings = new ApplicationSettings();
        var service = new RaceMenuPresetService(settings);
        service.RestoreState(new List<RaceMenuPreset> {
            new RaceMenuPreset { Name = "z", JslotFile = existingFile },
            new RaceMenuPreset { Name = "", JslotFile = existingFile },
            new RaceMenuPreset { Name = "a", JslotFile = Path.Combine(temp.Path, "missing.jslot") }
        });

        var only = Assert.Single(service.Presets);
        Assert.Equal("z", only.Name);
    }
}
