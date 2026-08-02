using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Body_Distribution_Studio.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class BodySlideViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    private readonly List<BodySlidePreset> _allPresets =
    [
        new() { Preset = "CBBE Curvy",    XmlFile = "CBBE.xml" },
        new() { Preset = "BHUNP Slim",    XmlFile = "BHUNP.xml" },
        new() { Preset = "CBBE 3BBB",     XmlFile = "CBBE3BBB.xml" },
        new() { Preset = "UUNP Special",  XmlFile = "UUNP.xml" },
    ];

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

    public BodySlideViewModel() => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredPresets.Clear();

        var source = string.IsNullOrWhiteSpace(_searchText)
            ? _allPresets.AsEnumerable()
            : _allPresets.Where(p =>
                p.Preset.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                p.XmlFile.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

        foreach (var preset in source)
            FilteredPresets.Add(preset);
    }
}
