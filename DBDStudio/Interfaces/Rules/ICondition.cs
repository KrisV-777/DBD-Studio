using System.Collections.ObjectModel;
using DBDStudio.Models.Component.Condition;
using System.ComponentModel;

namespace DBDStudio.Interfaces.Rules
{
    public enum ConditionType
    {
        GetActorsInHigh,
        GetActorValue,
        GetActorValuePercent,
        GetBaseActorValue,
        GetClothingValue,
        GetDead,
        GetDisease,
        GetFactionRank,
        GetGlobalValue,
        GetHighestRelationshipRank,
        GetInCurrentLoc,
        GetInCurrentLocFormList,
        GetInFaction,
        GetInWorldspace,
        GetIsClass,
        GetIsCrimeFaction,
        GetIsCurrentWeather,
        GetIsEditorLocation,
        GetIsGhost,
        GetIsID,
        GetIsRace,
        GetIsReference,
        GetIsSex,
        GetIsVoiceType,
        GetLevel,
        GetLowestRelationshipRank,
        GetPermanentActorValue,
        GetQuestCompleted,
        GetQuestRunning,
        GetRandomPercent,
        GetRealHoursPassed,
        GetRelationshipRank,
        GetStage,
        GetStageDone,
        GetTimeDead,
        HasBeenEaten,
        HasFamilyRelationship,
        HasKeyword,
        HasParentRelationship,
        HasPerk,
        IsChild,
        IsCloudy,
        IsCommandedActor,
        IsEssential,
        IsGuard,
        IsInList,
        IsPleasant,
        IsRaining,
        IsSnowing,
        IsUndead,
        IsUnique,
        WornHasKeyword
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

        ObservableCollection<ConditionValue> Arguments { get; }

        float? Comparator { get; set; }

        Conjunction Conjunction { get; set; }

        ICondition Copy();
    }
}
