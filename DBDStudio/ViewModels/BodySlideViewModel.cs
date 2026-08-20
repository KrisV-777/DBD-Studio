using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models.Component;

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
                ? _bodySlideService.Presets.AsEnumerable()
                : _bodySlideService.Presets.Where(p =>
                    p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.SourceXml.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var preset in source)
                FilteredPresets.Add(preset);
        }
    }
}
