using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DBDStudio.Interfaces;
using DBDStudio.Models;

namespace DBDStudio.ViewModels
{
    public sealed class RaceMenuPresetsViewModel : ViewModelBase
    {
        private readonly IRaceMenuPresetService _raceMenuPresetService;
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

        public ObservableCollection<RaceMenuPreset> FilteredPresets { get; } = [];
        public static IReadOnlyList<string> SexOptions { get; } = ["Male", "Female"];

        private void ApplyFilter()
        {
            FilteredPresets.Clear();

            var source = string.IsNullOrWhiteSpace(_searchText)
                ? _raceMenuPresetService.Presets.AsEnumerable()
                : _raceMenuPresetService.Presets.Where(p =>
                    p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.JsSlotFile.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.Sex.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var preset in source)
                FilteredPresets.Add(preset);
        }
    }
}
