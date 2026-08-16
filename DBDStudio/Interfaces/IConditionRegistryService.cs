using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Interfaces
{
    public interface IConditionRegistryService
    {
        IReadOnlyList<ConditionType> GetSupportedConditionTypes();
        int GetPriority(ConditionType type);
    }
}
