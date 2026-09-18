using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Services;

namespace DBDStudio.Tests;

public sealed class PersistenceRoundTripTests
{
    [Fact]
    public void PersistenceService_RestoresRulesWithPublishedPath()
    {
        using var temp = new TempDir();
        var workspacePath = Path.Combine(temp.Path, "workspace");
        var publishPath = Path.Combine(temp.Path, "saved-rule.json");

        var settings1 = new ApplicationSettings { WorkspaceFilePath = workspacePath };
        var ruleService1 = new RuleService(settings1);
        var created = ruleService1.EmplaceNew("PersistMe");
        ruleService1.SaveAs(created, publishPath);

        var persistenceSave = new PersistenceService(settings1, new IPersistable[] { settings1, ruleService1 });
        persistenceSave.Save();

        var settings2 = new ApplicationSettings { WorkspaceFilePath = workspacePath };
        var ruleService2 = new RuleService(settings2);
        var persistenceLoad = new PersistenceService(settings2, new IPersistable[] { settings2, ruleService2 });
        persistenceLoad.Load();

        var restored = Assert.Single(ruleService2.Rules);
        Assert.Equal(Path.GetFullPath(publishPath), restored.Underlying.LastPublishedPath);
        Assert.NotEqual(ConstructState.Ephemeral, restored.State);
    }
}
