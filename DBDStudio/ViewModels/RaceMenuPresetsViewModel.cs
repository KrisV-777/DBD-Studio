using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models.Component;
using DBDStudio.Utility;

namespace DBDStudio.ViewModels
{
    public sealed class RaceMenuPresetsViewModel : ViewModelBase
    {
        private readonly IRaceMenuPresetService _raceMenuPresetService;
        public ObservableCollection<RaceMenuPreset> FilteredPresets { get; } = [];
        private string _searchText = string.Empty;

        public RaceMenuPresetsViewModel(IRaceMenuPresetService raceMenuPresetService)
        {
            _raceMenuPresetService = raceMenuPresetService;
            _raceMenuPresetService.Presets.CollectionChanged += (_, _) => ApplyFilter();
            ApplyFilter();
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                    ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            CollectionFilter.ApplyTextFilter(
                FilteredPresets,
                _raceMenuPresetService.Presets,
                _searchText,
                preset => preset.Name,
                preset => preset.JslotFile);
        }
    }
}
