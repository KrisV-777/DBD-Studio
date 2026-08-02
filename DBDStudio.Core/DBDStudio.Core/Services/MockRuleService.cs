using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockRuleService : IRuleService
{
    private readonly List<Rule> _rules =
    [
        new()
        {
            Name = "Bandits",
            TexturePack = "Tempered",
            BodySlidePreset = "BHUNP Slim",
            RaceMenuPreset = "LydiaPreset",
            PriorityPreview = "Faction Match",
            Conditions =
            {
                new Condition { Type = "Faction", Operator = "==", Value = "Bandits" },
                new Condition { Type = "Sex", Operator = "==", Value = "Female" }
            }
        },
        new()
        {
            Name = "Companions",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            RaceMenuPreset = "WarriorMale",
            PriorityPreview = "Specific NPC",
            Conditions =
            {
                new Condition { Type = "Faction", Operator = "==", Value = "Companions" },
                new Condition { Type = "Race", Operator = "==", Value = "Nord" }
            }
        },
        new()
        {
            Name = "Unique NPC",
            TexturePack = "Player HD",
            BodySlidePreset = "UUNP Special",
            PriorityPreview = "Reference Match",
            Conditions =
            {
                new Condition { Type = "ReferenceID", Operator = "==", Value = "0x12345" }
            }
        },
        new()
        {
            Name = "Fallback",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            PriorityPreview = "Generic Fallback",
            Conditions =
            {
                new Condition { Type = "Sex", Operator = "==", Value = "Female" }
            }
        }
    ];

    public IReadOnlyList<Rule> GetRules() => _rules;

    public void Add(Rule rule) => _rules.Add(rule);

    public void Update(Rule rule)
    {
        var index = _rules.FindIndex(x => x.Name == rule.Name);
        if (index >= 0)
            _rules[index] = rule;
    }

    public void Remove(Rule rule) => _rules.Remove(rule);
}
