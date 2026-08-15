using DBDStudio.Models;

namespace DBDStudio.Interfaces
{
    public interface IConditionRegistryService
    {
        IReadOnlyList<ConditionDefinition> GetDefinitions();
        ConditionDefinition? FindByName(string? name);
    }
}
