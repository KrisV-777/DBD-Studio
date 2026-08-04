using System.Collections.ObjectModel;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.ViewModels;

public sealed class RaceMenuPresetsViewModel : ViewModelBase
{
    private readonly IRaceMenuPresetService _raceMenuPresetService;
    private string _searchText = string.Empty;
    private RaceMenuPreset? _selectedPreset;

    public RaceMenuPresetsViewModel(IRaceMenuPresetService raceMenuPresetService)
    {
        _raceMenuPresetService = raceMenuPresetService;
        AddPresetCommand = new RelayCommand(AddPreset);
        DeletePresetCommand = new RelayCommand(DeletePreset, () => SelectedPreset is not null);

        Refresh();
    }

    public ICommand AddPresetCommand { get; }
    public ICommand DeletePresetCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                Refresh();
        }
    }

    public RaceMenuPreset? SelectedPreset
    {
        get => _selectedPreset;
        set => SetField(ref _selectedPreset, value);
    }

    public ObservableCollection<RaceMenuPreset> Presets { get; } = [];
    public ObservableCollection<string> SexOptions { get; } = ["Male", "Female"];

    private void Refresh()
    {
        Presets.Clear();
        foreach (var preset in _raceMenuPresetService.GetPresets())
        {
            if (string.IsNullOrWhiteSpace(SearchText) ||
                preset.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                preset.JsSlotFile.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                preset.Sex.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                Presets.Add(preset);
            }
        }

        SelectedPreset ??= Presets.Count > 0 ? Presets[0] : null;
    }

    private void AddPreset()
    {
        var preset = new RaceMenuPreset { Name = "New Preset", Sex = "Male" };
        _raceMenuPresetService.Add(preset);
        Presets.Add(preset);
        SelectedPreset = preset;
    }

    private void DeletePreset()
    {
        if (SelectedPreset is null)
            return;

        _raceMenuPresetService.Remove(SelectedPreset);
        Presets.Remove(SelectedPreset);
        SelectedPreset = Presets.Count > 0 ? Presets[0] : null;
    }
}
