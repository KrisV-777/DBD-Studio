using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Services;

namespace DBDStudio.Tests;

public sealed class RuleServiceEdgeCaseTests
{
    [Fact]
    public void SaveAs_IgnoresWhitespacePath()
    {
        var service = new RuleService(new ApplicationSettings());
        var rule = service.EmplaceNew("A");

        service.SaveAs(rule, "   ");

        Assert.Single(service.Rules);
        Assert.Null(service.Rules[0].Underlying.LastPublishedPath);
    }

    [Fact]
    public void Remove_ThrowsForPrimordialRule()
    {
        using var temp = new TempDir();
        var service = new RuleService(new ApplicationSettings());
        var rule = service.EmplaceNew("A");
        service.SaveAs(rule, Path.Combine(temp.Path, "a.json"));

        var persisted = Assert.Single(service.Rules);
        Assert.Throws<InvalidOperationException>(() => service.Remove(persisted));
    }

    [Fact]
    public void Remove_RemovesEphemeralRule()
    {
        var service = new RuleService(new ApplicationSettings());
        var rule = service.EmplaceNew("A");

        service.Remove(rule);

        Assert.Empty(service.Rules);
    }

    [Fact]
    public void Reset_ThrowsForNonModifiedRule()
    {
        using var temp = new TempDir();
        var service = new RuleService(new ApplicationSettings());
        var path = Path.Combine(temp.Path, "a.json");
        var rule = service.EmplaceNew("A");
        service.SaveAs(rule, path);

        var persisted = Assert.Single(service.Rules);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(5));
        Assert.Throws<InvalidOperationException>(() => service.Reset(persisted));
    }

    [Fact]
    public void Reset_RestoresPrimordialValuesForModifiedRule()
    {
        using var temp = new TempDir();
        var service = new RuleService(new ApplicationSettings());
        var path = Path.Combine(temp.Path, "a.json");
        var rule = service.EmplaceNew("Before");
        service.SaveAs(rule, path);

        var persisted = Assert.Single(service.Rules);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddMinutes(-10));
        persisted.Underlying.Name = "After";
        Assert.Equal(ConstructState.Modified, persisted.State);

        service.Reset(persisted);

        Assert.Equal("Before", persisted.Underlying.Name);
    }

    [Fact]
    public void ResetRuleList_PicksNewestRuleVersionByUid()
    {
        var service = new RuleService(new ApplicationSettings());
        var uid = Guid.NewGuid();

        var older = new Rule { Name = "Old", LastPublishedPath = "C:/tmp/old.json", Uid = uid };
        var newer = new Rule { Name = "New", LastPublishedPath = "C:/tmp/new.json", Uid = uid };

        service.ResetRuleList(new List<Rule> { older, newer });

        var merged = Assert.Single(service.Rules);
        Assert.Equal("New", merged.Name);
        Assert.Equal("C:/tmp/new.json", merged.Underlying.LastPublishedPath);
    }
}
