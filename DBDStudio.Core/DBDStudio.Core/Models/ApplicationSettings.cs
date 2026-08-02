using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBDStudio.Core.Models;

public sealed class ApplicationSettings : INotifyPropertyChanged
{
    private string _workspaceFilePath = string.Empty;
    private string _skyrimDataFolder = string.Empty;
    private string _modsFolder = string.Empty;
    private string _bodySlidePresetsFolder = string.Empty;
    private string _raceMenuPresetsFolder = string.Empty;

    public string WorkspaceFilePath
    {
        get => _workspaceFilePath;
        set => SetProperty(ref _workspaceFilePath, value);
    }

    public string SkyrimDataFolder
    {
        get => _skyrimDataFolder;
        set => SetProperty(ref _skyrimDataFolder, value);
    }

    public string ModsFolder
    {
        get => _modsFolder;
        set => SetProperty(ref _modsFolder, value);
    }

    public string BodySlidePresetsFolder
    {
        get => _bodySlidePresetsFolder;
        set => SetProperty(ref _bodySlidePresetsFolder, value);
    }

    public string RaceMenuPresetsFolder
    {
        get => _raceMenuPresetsFolder;
        set => SetProperty(ref _raceMenuPresetsFolder, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
