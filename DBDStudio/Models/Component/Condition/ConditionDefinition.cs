using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Models.Component.Condition
{
    public sealed class ConditionDefinition
    {
        public static IEnumerable<ConditionValue> GetValuesForType(ConditionType type) => type switch {
            ConditionType.GetActorsInHigh => [],
            ConditionType.GetActorValue => [new ConditionValue.String()],
            ConditionType.GetActorValuePercent => [new ConditionValue.String()],
            ConditionType.GetBaseActorValue => [new ConditionValue.String()],
            ConditionType.GetClothingValue => [],
            ConditionType.GetDead => [],
            ConditionType.GetDisease => [],
            ConditionType.GetFactionRank => [new ConditionValue.Form(FormType.Faction)],
            ConditionType.GetGlobalValue => [new ConditionValue.Form(FormType.Global)],
            ConditionType.GetHighestRelationshipRank => [],
            ConditionType.GetInCurrentLoc => [new ConditionValue.Form(FormType.Location)],
            ConditionType.GetInCurrentLocFormList => [new ConditionValue.Form(FormType.FormList)],
            ConditionType.GetInFaction => [new ConditionValue.Form(FormType.Faction)],
            ConditionType.GetInWorldspace => [new ConditionValue.Form(FormType.Worldspace)],
            ConditionType.GetIsClass => [new ConditionValue.Form(FormType.Class)],
            ConditionType.GetIsCrimeFaction => [new ConditionValue.Form(FormType.Faction)],
            ConditionType.GetIsCurrentWeather => [new ConditionValue.Form(FormType.Weather)],
            ConditionType.GetIsEditorLocation => [new ConditionValue.Form(FormType.Location)],
            ConditionType.GetIsGhost => [],
            ConditionType.GetIsID => [new ConditionValue.Form(FormType.NPC)],
            ConditionType.GetIsRace => [new ConditionValue.Form(FormType.Race)],
            ConditionType.GetIsReference => [new ConditionValue.Form(FormType.ActorRef)],
            ConditionType.GetIsSex => [new ConditionValue.Sex()],
            ConditionType.GetIsVoiceType => [new ConditionValue.Form(FormType.VoiceType)],
            ConditionType.GetLevel => [],
            ConditionType.GetLowestRelationshipRank => [],
            ConditionType.GetPermanentActorValue => [new ConditionValue.String()],
            ConditionType.GetQuestCompleted => [new ConditionValue.Form(FormType.Quest)],
            ConditionType.GetQuestRunning => [new ConditionValue.Form(FormType.Quest)],
            ConditionType.GetRandomPercent => [],
            ConditionType.GetRealHoursPassed => [],
            ConditionType.GetRelationshipRank => [new ConditionValue.Form(FormType.ActorRef)],
            ConditionType.GetStage => [new ConditionValue.Form(FormType.Quest)],
            ConditionType.GetStageDone => [
                new ConditionValue.Form(FormType.Quest),
                new ConditionValue.Integer()
,            ],
            ConditionType.GetTimeDead => [],
            ConditionType.HasBeenEaten => [],
            ConditionType.HasFamilyRelationship => [new ConditionValue.Form(FormType.ActorRef)],
            ConditionType.HasKeyword => [new ConditionValue.Form(FormType.Keyword)],
            ConditionType.HasParentRelationship => [new ConditionValue.Form(FormType.ActorRef)],
            ConditionType.HasPerk => [new ConditionValue.Form(FormType.Perk)],
            ConditionType.IsChild => [],
            ConditionType.IsCloudy => [],
            ConditionType.IsCommandedActor => [],
            ConditionType.IsEssential => [],
            ConditionType.IsGuard => [],
            ConditionType.IsInList => [new ConditionValue.Form(FormType.FormList)],
            ConditionType.IsPleasant => [],
            ConditionType.IsRaining => [],
            ConditionType.IsSnowing => [],
            ConditionType.IsUndead => [],
            ConditionType.IsUnique => [],
            ConditionType.WornHasKeyword => [new ConditionValue.Form(FormType.Keyword)],
            _ => throw new NotImplementedException()
        };
    }
}
