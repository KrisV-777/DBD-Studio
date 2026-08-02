using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Body_Distribution_Studio.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class RulesViewModel : ViewModelBase
{
    private Rule? _selectedRule;
    private RuleCondition? _selectedCondition;

    public ObservableCollection<Rule> Rules { get; } = [];
    public ObservableCollection<string> AvailableTexturePacks { get; } = ["Fair Skin", "Tempered", "Custom"];
    public ObservableCollection<string> AvailableBodySlidePresets { get; } = ["CBBE Curvy", "BHUNP Slim"];
    public ObservableCollection<string> ConflictWarnings { get; } = [];

    public Rule? SelectedRule
    {
        get => _selectedRule;
        set
        {
            if (SetField(ref _selectedRule, value))
                SelectedCondition = null;
        }
    }

    public RuleCondition? SelectedCondition
    {
        get => _selectedCondition;
        set => SetField(ref _selectedCondition, value);
    }

    public ICommand AddRuleCommand { get; }
    public ICommand DuplicateRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand AddConditionCommand { get; }
    public ICommand RemoveConditionCommand { get; }
    public ICommand MoveConditionUpCommand { get; }
    public ICommand MoveConditionDownCommand { get; }

    public RulesViewModel()
    {
        AddRuleCommand = new RelayCommand(AddRule);
        DuplicateRuleCommand = new RelayCommand(DuplicateRule, () => SelectedRule is not null);
        DeleteRuleCommand = new RelayCommand(DeleteRule, () => SelectedRule is not null);
        AddConditionCommand = new RelayCommand(AddCondition, () => SelectedRule is not null);
        RemoveConditionCommand = new RelayCommand(RemoveCondition, () => SelectedRule is not null && SelectedCondition is not null);
        MoveConditionUpCommand = new RelayCommand(MoveConditionUp, CanMoveConditionUp);
        MoveConditionDownCommand = new RelayCommand(MoveConditionDown, CanMoveConditionDown);

        var nordFemales = new Rule
        {
            Name = "Nord Females",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            PriorityPreview = "Specific Reference"
        };
        nordFemales.Conditions.Add(new RuleCondition { Type = "Race",      Operator = "==", Value = "Nord" });
        nordFemales.Conditions.Add(new RuleCondition { Type = "Faction",   Operator = "==", Value = "Companions" });
        nordFemales.Conditions.Add(new RuleCondition { Type = "Sex",       Operator = "==", Value = "Female" });
        nordFemales.Conditions.Add(new RuleCondition { Type = "Reference", Operator = "==", Value = "0x12345" });

        var bandits = new Rule
        {
            Name = "Bandits",
            TexturePack = "Tempered",
            BodySlidePreset = "BHUNP Slim",
            PriorityPreview = "Faction Match"
        };
        bandits.Conditions.Add(new RuleCondition { Type = "Faction", Operator = "==", Value = "Bandits" });
        bandits.Conditions.Add(new RuleCondition { Type = "Sex",     Operator = "==", Value = "Female" });

        var fallback = new Rule
        {
            Name = "Fallback",
            TexturePack = "Fair Skin",
            BodySlidePreset = "CBBE Curvy",
            PriorityPreview = "Generic Fallback"
        };
        fallback.Conditions.Add(new RuleCondition { Type = "Sex", Operator = "==", Value = "Female" });

        Rules.Add(nordFemales);
        Rules.Add(bandits);
        Rules.Add(fallback);
        SelectedRule = nordFemales;

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
            PriorityPreview = SelectedRule.PriorityPreview
        };
        foreach (var c in SelectedRule.Conditions)
            copy.Conditions.Add(new RuleCondition { Type = c.Type, Operator = c.Operator, Value = c.Value });
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
        var cond = new RuleCondition { Type = "Race", Operator = "==", Value = "" };
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
        if (i > 0) SelectedRule.Conditions.Move(i, i - 1);
    }

    private void MoveConditionDown()
    {
        if (SelectedRule is null || SelectedCondition is null) return;
        var i = SelectedRule.Conditions.IndexOf(SelectedCondition);
        if (i < SelectedRule.Conditions.Count - 1)
            SelectedRule.Conditions.Move(i, i + 1);
    }

    private bool CanMoveConditionUp()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) > 0;

    private bool CanMoveConditionDown()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) < SelectedRule.Conditions.Count - 1;
}
