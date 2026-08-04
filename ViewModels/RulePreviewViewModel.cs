using System.Collections.ObjectModel;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.ViewModels;

public sealed class RulePreviewViewModel : ViewModelBase
{
    private readonly IFormDatabaseService _formDatabaseService;
    private readonly IRuleService _ruleService;
    private readonly IRuleResolutionService _ruleResolutionService;
    private FormReference? _selectedReference;
    private string _searchText = string.Empty;

    public RulePreviewViewModel(
        IFormDatabaseService formDatabaseService,
        IRuleService ruleService,
        IRuleResolutionService ruleResolutionService)
    {
        _formDatabaseService = formDatabaseService;
        _ruleService = ruleService;
        _ruleResolutionService = ruleResolutionService;
        ApplyCommand = new RelayCommand(ApplySelection);
    }

    public ICommand ApplyCommand { get; }
    public IFormDatabaseService FormDatabaseService => _formDatabaseService;

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public FormReference? SelectedReference
    {
        get => _selectedReference;
        set
        {
            if (SetField(ref _selectedReference, value))
                ApplySelection();
        }
    }

    public ObservableCollection<Rule> MatchingRules { get; } = [];
    public Rule? WinningRule { get; private set; }
    public string Priority { get; private set; } = "—";
    public string AssignedTexturePack { get; private set; } = "—";
    public string AssignedBodySlidePreset { get; private set; } = "—";
    public string AssignedRaceMenuPreset { get; private set; } = "—";

    private void ApplySelection()
    {
        MatchingRules.Clear();
        WinningRule = null;
        Priority = "—";
        AssignedTexturePack = "—";
        AssignedBodySlidePreset = "—";
        AssignedRaceMenuPreset = "—";

        if (_selectedReference is null)
            return;

        var rules = _ruleService.GetRules();
        foreach (var rule in rules)
            MatchingRules.Add(rule);

        WinningRule = _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.Texture)
            ?? _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.BodySlide)
            ?? _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.RaceMenu);

        if (WinningRule is not null)
        {
            Priority = _ruleResolutionService.GetDerivedPriority(WinningRule).ToString();
        }

        var textureRule = _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.Texture);
        AssignedTexturePack = textureRule is null
            ? "No assignment"
            : _ruleResolutionService.ResolveWinningCandidate(textureRule, AssignmentCategory.Texture) ?? "No assignment";

        var bodySlideRule = _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.BodySlide);
        AssignedBodySlidePreset = bodySlideRule is null
            ? "No assignment"
            : _ruleResolutionService.ResolveWinningCandidate(bodySlideRule, AssignmentCategory.BodySlide) ?? "No assignment";

        var raceMenuRule = _ruleResolutionService.ResolveWinningRule(rules, AssignmentCategory.RaceMenu);
        AssignedRaceMenuPreset = raceMenuRule is null
            ? "No assignment"
            : _ruleResolutionService.ResolveWinningCandidate(raceMenuRule, AssignmentCategory.RaceMenu) ?? "No assignment";

        OnPropertyChanged(nameof(MatchingRules));
        OnPropertyChanged(nameof(WinningRule));
        OnPropertyChanged(nameof(Priority));
        OnPropertyChanged(nameof(AssignedTexturePack));
        OnPropertyChanged(nameof(AssignedBodySlidePreset));
        OnPropertyChanged(nameof(AssignedRaceMenuPreset));
    }
}
