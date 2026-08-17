using DBDStudio.Interfaces;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Services
{
    public sealed class ConditionRegistryService : IConditionRegistryService
    {
        private static readonly Dictionary<ConditionType, int> Priorities = new()
        {
            [ConditionType.IsRace] = 0,
            [ConditionType.IsSex] = 0,
            [ConditionType.GetLevel] = 0,
            [ConditionType.HasKeyword] = 1,
            [ConditionType.IsInFaction] = 2,
            [ConditionType.GetFactionRank] = 3,
            [ConditionType.IsNPC] = 4,
            [ConditionType.IsReference] = 5,
            [ConditionType.HasPerk] = 1,
            [ConditionType.IsInFormList] = 1,
            [ConditionType.UsesCombatStyle] = 1
        };

        private static readonly IReadOnlyList<ConditionType> SupportedTypes =
            Enum.GetValues<ConditionType>();

        public IReadOnlyList<ConditionType> GetSupportedConditionTypes() => SupportedTypes;

        public int GetPriority(ConditionType type) => Priorities.TryGetValue(type, out var priority) ? priority : 0;
    }
}
