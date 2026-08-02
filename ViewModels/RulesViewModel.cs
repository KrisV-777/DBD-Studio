using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class RulesViewModel : ViewModelBase
{
    private readonly IRuleService _ruleService;
    private readonly IRaceMenuPresetService _raceMenuPresetService;
    private Rule? _selectedRule;
    private Condition? _selectedCondition;
    private string _raceMenuAssignmentWarning = string.Empty;

    public ObservableCollection<Rule> Rules { get; } = [];
    public ObservableCollection<string> AvailableTexturePacks { get; } = ["Fair Skin", "Tempered", "Custom", "Player HD"];
    public ObservableCollection<string> AvailableBodySlidePresets { get; } = ["CBBE Curvy", "BHUNP Slim", "UUNP Special"];
    public ObservableCollection<string> AvailableRaceMenuPresets { get; } = [];
    public ObservableCollection<string> AvailableOperators { get; } = ["<", "<=", "==", ">=", ">", "!="];
    public ObservableCollection<string> ConflictWarnings { get; } = [];

    public Rule? SelectedRule
    {
        get => _selectedRule;
        set
        {
           if (SetField(ref _selectedRule, value))
           {
               SelectedCondition = null;
               UpdateRaceMenuWarning();
           }
        }
    }

    public Condition? SelectedCondition
    {
        get => _selectedCondition;
        set => SetField(ref _selectedCondition, value);
    }

    public string RaceMenuAssignmentWarning
    {
        get => _raceMenuAssignmentWarning;
        private set => SetField(ref _raceMenuAssignmentWarning, value);
    }

    public ICommand AddRuleCommand { get; }
    public ICommand DuplicateRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand AddConditionCommand { get; }
    public ICommand RemoveConditionCommand { get; }
    public ICommand MoveConditionUpCommand { get; }
    public ICommand MoveConditionDownCommand { get; }

    public RulesViewModel(IRuleService ruleService, IRaceMenuPresetService raceMenuPresetService)
    {
        _ruleService = ruleService;
        _raceMenuPresetService = raceMenuPresetService;
        AddRuleCommand = new RelayCommand(AddRule);
        DuplicateRuleCommand = new RelayCommand(DuplicateRule, () => SelectedRule is not null);
        DeleteRuleCommand = new RelayCommand(DeleteRule, () => SelectedRule is not null);
        AddConditionCommand = new RelayCommand(AddCondition, () => SelectedRule is not null);
        RemoveConditionCommand = new RelayCommand(RemoveCondition, () => SelectedRule is not null && SelectedCondition is not null);
        MoveConditionUpCommand = new RelayCommand(MoveConditionUp, CanMoveConditionUp);
        MoveConditionDownCommand = new RelayCommand(MoveConditionDown, CanMoveConditionDown);

        foreach (var preset in raceMenuPresetService.GetPresets())
           AvailableRaceMenuPresets.Add(preset.Name);

        foreach (var rule in _ruleService.GetRules())
        {
           var uiRule = new Rule
           {
               Name = rule.Name,
               TexturePack = rule.TexturePack,
               BodySlidePreset = rule.BodySlidePreset,
               RaceMenuPreset = rule.RaceMenuPreset,
               PriorityPreview = rule.PriorityPreview
           };
           foreach (var condition in rule.Conditions)
               uiRule.Conditions.Add(new Condition { Type = condition.Type, Operator = condition.Operator, Value = condition.Value });

           Rules.Add(uiRule);
        }

        SelectedRule = Rules.Count > 0 ? Rules[0] : null;

        ConflictWarnings.Add("Conflicts with \"Bandits\" — overlapping conditions");
        ConflictWarnings.Add("Winning Rule: Specific NPC Assignment (higher priority)");
    }

    private void AddRule()
    {
        var rule = new Rule { Name = "New Rule" };
        Rules.Add(rule);
        SelectedRule = rule;
    }

    private void DuplicateRule()
    {
        if (SelectedRule is null) return;
        var copy = new Rule
        {
           Name = SelectedRule.Name + " (Copy)",
           TexturePack = SelectedRule.TexturePack,
           BodySlidePreset = SelectedRule.BodySlidePreset,
           RaceMenuPreset = SelectedRule.RaceMenuPreset,
           PriorityPreview = SelectedRule.PriorityPreview
        };
        foreach (var c in SelectedRule.Conditions)
           copy.Conditions.Add(new Condition { Type = c.Type, Operator = c.Operator, Value = c.Value });
        Rules.Add(copy);
        SelectedRule = copy;
    }

    private void DeleteRule()
    {
        if (SelectedRule is null) return;
        var index = Rules.IndexOf(SelectedRule);
        Rules.Remove(SelectedRule);
        SelectedRule = Rules.Count > 0 ? Rules[Math.Max(0, index - 1)] : null;
    }

    private void AddCondition()
    {
        if (SelectedRule is null) return;
        var cond = new Condition { Type = "Race", Operator = "==", Value = string.Empty };
        SelectedRule.Conditions.Add(cond);
        SelectedCondition = cond;
    }

    private void RemoveCondition()
    {
        if (SelectedRule is null || SelectedCondition is null) return;
        SelectedRule.Conditions.Remove(SelectedCondition);
        SelectedCondition = null;
    }

    private void MoveConditionUp()
    {
        if (SelectedRule is null || SelectedCondition is null) return;
        var i = SelectedRule.Conditions.IndexOf(SelectedCondition);
        if (i > 0)
        {
           var current = SelectedRule.Conditions[i];
           SelectedRule.Conditions[i] = SelectedRule.Conditions[i - 1];
           SelectedRule.Conditions[i - 1] = current;
        }
    }

    private void MoveConditionDown()
    {
        if (SelectedRule is null || SelectedCondition is null) return;
        var i = SelectedRule.Conditions.IndexOf(SelectedCondition);
        if (i < SelectedRule.Conditions.Count - 1)
        {
           var current = SelectedRule.Conditions[i];
           SelectedRule.Conditions[i] = SelectedRule.Conditions[i + 1];
           SelectedRule.Conditions[i + 1] = current;
        }
    }

    private bool CanMoveConditionUp()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) > 0;

    private bool CanMoveConditionDown()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) < SelectedRule.Conditions.Count - 1;

    private void UpdateRaceMenuWarning()
    {
        if (SelectedRule is null || SelectedRule.RaceMenuAssignment is null)
        {
           RaceMenuAssignmentWarning = string.Empty;
           return;
        }

        var hasReferenceCondition = SelectedRule.Conditions.Any(c => c.Type.Equals("ReferenceID", StringComparison.OrdinalIgnoreCase));
        var hasUniqueActorBaseCondition = SelectedRule.Conditions.Any(c =>
           c.Type.Equals("ActorBase", StringComparison.OrdinalIgnoreCase) &&
           c.Value.Contains("Unique", StringComparison.OrdinalIgnoreCase));

        RaceMenuAssignmentWarning = hasReferenceCondition || hasUniqueActorBaseCondition
           ? string.Empty
           : "RaceMenu presets require either:\n- ReferenceID condition\n- Unique ActorBase condition";
    }
}
