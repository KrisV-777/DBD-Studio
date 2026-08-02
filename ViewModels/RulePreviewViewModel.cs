using System.Collections.ObjectModel;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class RulePreviewViewModel : ViewModelBase
{
    private readonly IFormDatabaseService _formDatabaseService;
    private readonly IRuleService _ruleService;
    private FormReference? _selectedReference;
    private string _searchText = string.Empty;

    public RulePreviewViewModel(IFormDatabaseService formDatabaseService, IRuleService ruleService)
    {
        _formDatabaseService = formDatabaseService;
        _ruleService = ruleService;
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

        WinningRule = rules[0];
        Priority = WinningRule.PriorityPreview;
        AssignedTexturePack = string.IsNullOrWhiteSpace(WinningRule.TexturePack) ? "No assignment" : WinningRule.TexturePack;
        AssignedBodySlidePreset = string.IsNullOrWhiteSpace(WinningRule.BodySlidePreset) ? "No assignment" : WinningRule.BodySlidePreset;
        AssignedRaceMenuPreset = string.IsNullOrWhiteSpace(WinningRule.RaceMenuPreset) ? "No assignment" : WinningRule.RaceMenuPreset;

        OnPropertyChanged(nameof(MatchingRules));
        OnPropertyChanged(nameof(WinningRule));
        OnPropertyChanged(nameof(Priority));
        OnPropertyChanged(nameof(AssignedTexturePack));
        OnPropertyChanged(nameof(AssignedBodySlidePreset));
        OnPropertyChanged(nameof(AssignedRaceMenuPreset));
    }
}
