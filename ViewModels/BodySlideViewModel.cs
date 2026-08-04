using System;
using System.Collections.ObjectModel;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.ViewModels;

public sealed class BodySlideViewModel : ViewModelBase
{
    private readonly IBodySlideService _bodySlideService;
    private string _searchText = string.Empty;

    public ObservableCollection<BodySlidePreset> FilteredPresets { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                ApplyFilter();
        }
    }

    public BodySlideViewModel(IBodySlideService bodySlideService)
    {
        _bodySlideService = bodySlideService;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredPresets.Clear();

        var source = string.IsNullOrWhiteSpace(_searchText)
            ? _bodySlideService.GetPresets().AsEnumerable()
            : _bodySlideService.GetPresets().Where(p =>
                p.Preset.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                p.SourceXml.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

        foreach (var preset in source)
            FilteredPresets.Add(preset);
    }
}
