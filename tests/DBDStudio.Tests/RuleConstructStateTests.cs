using DBDStudio.Models;
using DBDStudio.Models.Component;

namespace DBDStudio.Tests;

public sealed class RuleConstructStateTests
{
    [Fact]
    public void State_IsEphemeral_WhenNoPublishedPath()
    {
        var rule = new Rule { Name = "NoPath" };
        var construct = new RuleConstruct(rule, isPrimordial: true);

        Assert.Equal(ConstructState.Ephemeral, construct.State);
    }

    [Fact]
    public void State_IsEphemeral_WhenPublishedFileMissing()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        var rule = new Rule { Name = "Missing", LastPublishedPath = missingPath };
        var construct = new RuleConstruct(rule, isPrimordial: true);

        Assert.Equal(ConstructState.Ephemeral, construct.State);
    }

    [Fact]
    public void State_IsPrimordial_WhenPublishedFileIsNewerOrEqual()
    {
        using var temp = new TempDir();
        var filePath = Path.Combine(temp.Path, "rule.json");
        File.WriteAllText(filePath, "{}");

        var rule = new Rule { Name = "Primordial", LastPublishedPath = filePath };
        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddMinutes(1));

        var construct = new RuleConstruct(rule, isPrimordial: true);
        Assert.Equal(ConstructState.Primordial, construct.State);
    }

    [Fact]
    public void State_IsModified_WhenLocalRuleIsNewerThanPublishedFile()
    {
        using var temp = new TempDir();
        var filePath = Path.Combine(temp.Path, "rule.json");
        File.WriteAllText(filePath, "{}");

        var rule = new Rule { Name = "Modified", LastPublishedPath = filePath };
        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow.AddMinutes(-5));

        var construct = new RuleConstruct(rule, isPrimordial: true);
        Assert.Equal(ConstructState.Modified, construct.State);
    }
}
