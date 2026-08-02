namespace Body_Distribution_Studio.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private string _skyrimDataFolder = string.Empty;
    private string _modsFolder = string.Empty;
    private string _bodySlidePresetsFolder = string.Empty;

    public string SkyrimDataFolder
    {
        get => _skyrimDataFolder;
        set => SetField(ref _skyrimDataFolder, value);
    }

    public string ModsFolder
    {
        get => _modsFolder;
        set => SetField(ref _modsFolder, value);
    }

    public string BodySlidePresetsFolder
    {
        get => _bodySlidePresetsFolder;
        set => SetField(ref _bodySlidePresetsFolder, value);
    }

    // Placeholder info-card values — will come from the asset scanner later.
    public int TexturePacksFound => 3;
    public int BodySlidePresetsFound => 2;
    public string LastScanTime => "2024-01-15  14:32";
}
