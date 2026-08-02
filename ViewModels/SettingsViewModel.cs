using System.Windows.Input;
using DBDStudio.Core.Interfaces;

namespace Body_Distribution_Studio.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _settingsService.Settings.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
        SaveCommand = new RelayCommand(Save);
    }

    public ICommand SaveCommand { get; }

    public string SkyrimDataFolder
    {
        get => _settingsService.Settings.SkyrimDataFolder;
        set
        {
            _settingsService.Settings.SkyrimDataFolder = value;
            OnPropertyChanged();
        }
    }

    public string ModsFolder
    {
        get => _settingsService.Settings.ModsFolder;
        set
        {
            _settingsService.Settings.ModsFolder = value;
            OnPropertyChanged();
        }
    }

    public string BodySlidePresetsFolder
    {
        get => _settingsService.Settings.BodySlidePresetsFolder;
        set
        {
            _settingsService.Settings.BodySlidePresetsFolder = value;
            OnPropertyChanged();
        }
    }

    public string RaceMenuPresetsFolder
    {
        get => _settingsService.Settings.RaceMenuPresetsFolder;
        set
        {
            _settingsService.Settings.RaceMenuPresetsFolder = value;
            OnPropertyChanged();
        }
    }

    public string WorkspaceFilePath
    {
        get => _settingsService.Settings.WorkspaceFilePath;
        set
        {
            _settingsService.Settings.WorkspaceFilePath = value;
            OnPropertyChanged();
        }
    }

    public int TexturePacksFound => 4;
    public int BodySlidePresetsFound => 3;
    public string LastScanTime => "Ready for scan";

    private void Save() => _settingsService.Save();
}
