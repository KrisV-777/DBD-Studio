using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Models.Component.Condition
{
    public sealed class ConditionDefinition
    {
        public static IEnumerable<ConditionValue> GetValuesForType(ConditionType type)
        {
            switch (type)
            {
                case ConditionType.IsReference: yield return new ConditionValue.Form(); break;
                case ConditionType.IsNPC: yield return new ConditionValue.Form(); break;
                case ConditionType.HasPerk: yield return new ConditionValue.Form(); break;
                case ConditionType.IsRace: yield return new ConditionValue.Form(); break;
                case ConditionType.IsInFormList: yield return new ConditionValue.Form(); break;
                case ConditionType.IsInFaction: yield return new ConditionValue.Form(); break;
                case ConditionType.GetFactionRank:
                    yield return new ConditionValue.Form();
                    yield return new ConditionValue.Integer();
                    break;
                case ConditionType.UsesCombatStyle: yield return new ConditionValue.Form(); break;
                case ConditionType.HasKeyword: yield return new ConditionValue.Form(); break;
                case ConditionType.IsSex: yield return new ConditionValue.Sex(); break;
                case ConditionType.GetLevel: yield return new ConditionValue.Integer(); break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static IEnumerable<FormType> GetFormTypeForType(ConditionType type)
        {
            switch (type)
            {
                case ConditionType.IsReference: yield return FormType.ActorRef; break;
                case ConditionType.IsNPC: yield return FormType.NPC; break;
                case ConditionType.HasPerk: yield return FormType.Perk; break;
                case ConditionType.IsRace: yield return FormType.Race; break;
                case ConditionType.IsInFormList: yield return FormType.FormList; break;
                case ConditionType.IsInFaction: yield return FormType.Faction; break;
                case ConditionType.GetFactionRank:
                    yield return FormType.Faction;
                    yield return FormType.None;
                    break;
                case ConditionType.UsesCombatStyle: yield return FormType.CombatStyle; break;
                case ConditionType.HasKeyword: yield return FormType.Keyword; break;
                case ConditionType.IsSex: yield return FormType.None; break;
                case ConditionType.GetLevel: yield return FormType.None; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
