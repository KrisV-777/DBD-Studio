using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Models.Component;

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
            FilteredPresets.Clear();

            var source = string.IsNullOrWhiteSpace(_searchText)
                ? _raceMenuPresetService.Presets.AsEnumerable()
                : _raceMenuPresetService.Presets.Where(p =>
                    p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.JslotFile.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var preset in source)
                FilteredPresets.Add(preset);
        }
    }
}
