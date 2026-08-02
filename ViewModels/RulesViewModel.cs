using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class RulesViewModel : ViewModelBase
{
    private readonly IRuleService _ruleService;
    private readonly IRaceMenuPresetService _raceMenuPresetService;
    private readonly IConditionRegistryService _conditionRegistryService;
    private readonly IRuleResolutionService _ruleResolutionService;
    private Rule? _selectedRule;
    private Condition? _selectedCondition;
    private string _raceMenuAssignmentWarning = string.Empty;

    public ObservableCollection<Rule> Rules { get; } = [];
    public ObservableCollection<string> AvailableTexturePacks { get; } = ["Fair Skin", "Tempered", "Custom", "Player HD"];
    public ObservableCollection<string> AvailableBodySlidePresets { get; } = ["CBBE Curvy", "BHUNP Slim", "UUNP Special"];
    public ObservableCollection<string> AvailableRaceMenuPresets { get; } = [];
    public ObservableCollection<string> AvailableConditionTypes { get; } = [];
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
                OnPropertyChanged(nameof(DerivedPriority));
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

    public int DerivedPriority => SelectedRule is null ? 0 : _ruleResolutionService.GetDerivedPriority(SelectedRule);

    public ICommand AddRuleCommand { get; }
    public ICommand DuplicateRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand AddConditionCommand { get; }
    public ICommand RemoveConditionCommand { get; }
    public ICommand MoveConditionUpCommand { get; }
    public ICommand MoveConditionDownCommand { get; }

    public RulesViewModel(
        IRuleService ruleService,
        IRaceMenuPresetService raceMenuPresetService,
        IConditionRegistryService conditionRegistryService,
        IRuleResolutionService ruleResolutionService)
    {
        _ruleService = ruleService;
        _raceMenuPresetService = raceMenuPresetService;
        _conditionRegistryService = conditionRegistryService;
        _ruleResolutionService = ruleResolutionService;

        AddRuleCommand = new RelayCommand(AddRule);
        DuplicateRuleCommand = new RelayCommand(DuplicateRule, () => SelectedRule is not null);
        DeleteRuleCommand = new RelayCommand(DeleteRule, () => SelectedRule is not null);
        AddConditionCommand = new RelayCommand(AddCondition, () => SelectedRule is not null);
        RemoveConditionCommand = new RelayCommand(RemoveCondition, () => SelectedRule is not null && SelectedCondition is not null);
        MoveConditionUpCommand = new RelayCommand(MoveConditionUp, CanMoveConditionUp);
        MoveConditionDownCommand = new RelayCommand(MoveConditionDown, CanMoveConditionDown);

        foreach (var definition in _conditionRegistryService.GetDefinitions().OrderBy(d => d.Priority).ThenBy(d => d.DisplayName))
            AvailableConditionTypes.Add(definition.Name);

        foreach (var preset in raceMenuPresetService.GetPresets())
            AvailableRaceMenuPresets.Add(preset.Name);

        foreach (var rule in _ruleService.GetRules())
        {
            AttachRule(rule);
            Rules.Add(rule);
        }

        SelectedRule = Rules.Count > 0 ? Rules[0] : null;
        RefreshConflictWarnings();
    }

    private void AddRule()
    {
        var rule = new Rule { Name = "New Rule", FileName = "New Rule.yaml" };
        _ruleService.Add(rule);
        AttachRule(rule);
        Rules.Add(rule);
        SelectedRule = rule;
        RefreshConflictWarnings();
    }

    private void DuplicateRule()
    {
        if (SelectedRule is null)
            return;

        var copy = new Rule
        {
            Name = SelectedRule.Name + " (Copy)",
            FileName = SelectedRule.FileName + ".copy"
        };
        foreach (var texture in SelectedRule.TextureCandidates)
            copy.TextureCandidates.Add(texture);
        foreach (var preset in SelectedRule.BodySlideCandidates)
            copy.BodySlideCandidates.Add(preset);
        foreach (var preset in SelectedRule.RaceMenuCandidates)
            copy.RaceMenuCandidates.Add(preset);
        foreach (var condition in SelectedRule.Conditions)
            copy.Conditions.Add(new Condition { Type = condition.Type, Operator = condition.Operator, Value = condition.Value, Group = condition.Group });

        _ruleService.Add(copy);
        AttachRule(copy);
        Rules.Add(copy);
        SelectedRule = copy;
        RefreshConflictWarnings();
    }

    private void DeleteRule()
    {
        if (SelectedRule is null)
            return;

        var index = Rules.IndexOf(SelectedRule);
        DetachRule(SelectedRule);
        _ruleService.Remove(SelectedRule);
        Rules.Remove(SelectedRule);
        SelectedRule = Rules.Count > 0 ? Rules[Math.Max(0, index - 1)] : null;
        RefreshConflictWarnings();
    }

    private void AddCondition()
    {
        if (SelectedRule is null)
            return;

        var defaultType = AvailableConditionTypes.Count > 0 ? AvailableConditionTypes[0] : "Race";
        var condition = new Condition { Type = defaultType, Operator = "==", Value = string.Empty, Group = 0 };
        condition.PropertyChanged += OnConditionChanged;
        SelectedRule.Conditions.Add(condition);
        SelectedCondition = condition;
        UpdateDerivedPriorityPreview();
        UpdateRaceMenuWarning();
    }

    private void RemoveCondition()
    {
        if (SelectedRule is null || SelectedCondition is null)
            return;

        SelectedCondition.PropertyChanged -= OnConditionChanged;
        SelectedRule.Conditions.Remove(SelectedCondition);
        SelectedCondition = null;
        UpdateDerivedPriorityPreview();
        UpdateRaceMenuWarning();
    }

    private void MoveConditionUp()
    {
        if (SelectedRule is null || SelectedCondition is null)
            return;

        var index = SelectedRule.Conditions.IndexOf(SelectedCondition);
        if (index > 0)
            SelectedRule.Conditions.Move(index, index - 1);
    }

    private void MoveConditionDown()
    {
        if (SelectedRule is null || SelectedCondition is null)
            return;

        var index = SelectedRule.Conditions.IndexOf(SelectedCondition);
        if (index < SelectedRule.Conditions.Count - 1)
            SelectedRule.Conditions.Move(index, index + 1);
    }

    private bool CanMoveConditionUp()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) > 0;

    private bool CanMoveConditionDown()
        => SelectedRule is not null && SelectedCondition is not null
           && SelectedRule.Conditions.IndexOf(SelectedCondition) < SelectedRule.Conditions.Count - 1;

    private void UpdateRaceMenuWarning()
    {
        if (SelectedRule is null || SelectedRule.RaceMenuCandidates.Count == 0)
        {
            RaceMenuAssignmentWarning = string.Empty;
            return;
        }

        var hasReferenceCondition = SelectedRule.Conditions.Any(c => c.Type.Equals("ReferenceID", StringComparison.OrdinalIgnoreCase));
        var hasActorBaseCondition = SelectedRule.Conditions.Any(c => c.Type.Equals("ActorBase", StringComparison.OrdinalIgnoreCase));
        RaceMenuAssignmentWarning = hasReferenceCondition || hasActorBaseCondition
            ? string.Empty
            : "RaceMenu presets require either:\n- ReferenceID condition\n- ActorBase condition";
    }

    private void RefreshConflictWarnings()
    {
        ConflictWarnings.Clear();
        var groupedByName = Rules.GroupBy(rule => rule.Name, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1);
        foreach (var duplicate in groupedByName)
            ConflictWarnings.Add($"Duplicate rule name detected: {duplicate.Key}");

        if (ConflictWarnings.Count == 0)
            ConflictWarnings.Add("No obvious naming conflicts found.");
    }

    private void AttachRule(Rule rule)
    {
        foreach (var condition in rule.Conditions)
            condition.PropertyChanged += OnConditionChanged;

        rule.PriorityPreview = BuildPriorityPreview(rule);
    }

    private void DetachRule(Rule rule)
    {
        foreach (var condition in rule.Conditions)
            condition.PropertyChanged -= OnConditionChanged;
    }

    private void OnConditionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Condition.Type) or nameof(Condition.Operator) or nameof(Condition.Value) or nameof(Condition.Group))
        {
            UpdateDerivedPriorityPreview();
            UpdateRaceMenuWarning();
        }
    }

    private void UpdateDerivedPriorityPreview()
    {
        if (SelectedRule is null)
            return;

        SelectedRule.PriorityPreview = BuildPriorityPreview(SelectedRule);
        OnPropertyChanged(nameof(DerivedPriority));
    }

    private string BuildPriorityPreview(Rule rule)
    {
        var priority = _ruleResolutionService.GetDerivedPriority(rule);
        return $"Derived from max condition priority: {priority}";
    }
}
