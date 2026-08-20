using System.Collections.ObjectModel;
using DBDStudio.Models.Component.Condition;
using System.ComponentModel;

namespace DBDStudio.Interfaces.Rules
{
    public enum ConditionType
    {
        IsReference,
        IsNPC,
        HasPerk,
        IsRace,
        IsInFormList,
        IsInFaction,
        UsesCombatStyle,
        HasKeyword,
        GetFactionRank,
        IsSex,
        GetLevel
    }

    public enum Operator
    {
        Equals, // ==
        NotEquals, // !=
        GreaterThan, // >
        LessThan, // <
        GreaterThanOrEqual, // >=
        LessThanOrEqual // <=
    }

    public enum Conjunction
    {
        And,
        Or
    }

    public interface ICondition : INotifyPropertyChanged
    {
        ConditionType ConditionType { get; set; }

        Operator Operator { get; set; }

        ObservableCollection<ConditionValue> Values { get; }

        Conjunction Conjunction { get; set; }

        ICondition Copy();
    }
}
