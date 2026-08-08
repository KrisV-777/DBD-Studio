using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IConditionRegistryService
    {
        IReadOnlyList<ConditionDefinition> GetDefinitions();
        ConditionDefinition? FindByName(string? name);
    }
}
