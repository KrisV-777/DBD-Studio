using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models.Component;
using DBDStudio.Utility;

namespace DBDStudio.ViewModels
{
    public sealed class BodySlideViewModel : ViewModelBase
    {
        private readonly IBodySlideService _bodySlideService;
        public ObservableCollection<BodySlidePreset> FilteredPresets { get; } = [];
        private string _searchText = string.Empty;

        public BodySlideViewModel(IBodySlideService bodySlideService)
        {
            _bodySlideService = bodySlideService;
            _bodySlideService.Presets.CollectionChanged += (_, _) => ApplyFilter();
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
                _bodySlideService.Presets,
                _searchText,
                preset => preset.Name,
                preset => preset.SourceXml);
        }
    }
}
