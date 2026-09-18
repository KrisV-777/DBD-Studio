using System.Text.Json;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Services;

namespace DBDStudio.Tests;

public sealed class RuleServiceTests
{
    [Fact]
    public void Save_Throws_ForEphemeralRule()
    {
        var service = new RuleService(new ApplicationSettings());
        var rule = service.EmplaceNew();

        var ex = Assert.Throws<InvalidOperationException>(() => service.Save(rule));
        Assert.Contains("ephemeral", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SaveAs_PublishesRule_UpdatesPath_AndOmitsLocalPathFromExport()
    {
        using var temp = new TempDir();
        var service = new RuleService(new ApplicationSettings());
        var pathWithoutExtension = Path.Combine(temp.Path, "published-rule");

        var created = service.EmplaceNew("PublishMe");
        created.Underlying.RaceMenuCandidate = "preset.jslot";

        service.SaveAs(created, pathWithoutExtension);

        var persisted = Assert.Single(service.Rules);
        var normalizedPath = Path.GetFullPath(pathWithoutExtension + ".json");
        Assert.Equal(normalizedPath, persisted.Underlying.LastPublishedPath);
        Assert.NotEqual(ConstructState.Ephemeral, persisted.State);
        Assert.True(File.Exists(normalizedPath));

        using var document = JsonDocument.Parse(File.ReadAllText(normalizedPath));
        Assert.False(document.RootElement.TryGetProperty("LastPublishedPath", out _));
    }

    [Fact]
    public void Save_OverwritesMostRecentPublishedPath()
    {
        using var temp = new TempDir();
        var service = new RuleService(new ApplicationSettings());
        var publishPath = Path.Combine(temp.Path, "rule.json");

        var created = service.EmplaceNew("Initial");
        service.SaveAs(created, publishPath);

        var persisted = Assert.Single(service.Rules);
        persisted.Underlying.Name = "AfterEdit";

        service.Save(persisted);

        var savedAgain = Assert.Single(service.Rules);
        Assert.NotEqual(ConstructState.Ephemeral, savedAgain.State);

        using var document = JsonDocument.Parse(File.ReadAllText(publishPath));
        var exportedName = document.RootElement.GetProperty("Name").GetString();
        Assert.Equal("AfterEdit", exportedName);
    }
}
