using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using System.Diagnostics;
using Avalonia;
using Avalonia.Styling;

namespace DBDStudio.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ITexturePackService _texturePackService;
    private readonly IBodySlideService _bodySlideService;
    private readonly IRaceMenuPresetService _raceMenuPresetService;
    private readonly MainWindowViewModel? _mainWindowViewModel;

    public static readonly string[] ThemeOptions = ["Light", "Dark", "System"];

    public SettingsViewModel(
        ISettingsService settingsService,
        ITexturePackService texturePackService,
        IBodySlideService bodySlideService,
        IRaceMenuPresetService raceMenuPresetService,
        MainWindowViewModel? mainWindowViewModel = null)
    {
        _settingsService = settingsService;
        _texturePackService = texturePackService;
        _bodySlideService = bodySlideService;
        _raceMenuPresetService = raceMenuPresetService;
        _mainWindowViewModel = mainWindowViewModel;

        _settingsService.Settings.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
        _texturePackService.TexturePacksChanged += (_, _) => OnPropertyChanged(nameof(TexturePacksFound));

        SaveCommand = new RelayCommand(() => _settingsService.Save());
        CmdOpenGithub = new RelayCommand(() => OpenUrl("https://github.com/"));
        CmdOpenWiki = new RelayCommand(() => OpenUrl("https://github.com/wiki"));
        CmdOpenNexus = new RelayCommand(() => OpenUrl("https://www.nexusmods.com/"));
        CmdOpenKofi = new RelayCommand(() => OpenUrl("https://ko-fi.com/"));
    }

    static private void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public ICommand SaveCommand { get; }
    public ICommand CmdOpenGithub { get; }
    public ICommand CmdOpenWiki { get; }
    public ICommand CmdOpenNexus { get; }
    public ICommand CmdOpenKofi { get; }

    public static string GitHubIconPath => (Application.Current?.ActualThemeVariant ?? ThemeVariant.Light) == ThemeVariant.Dark
        ? "/Assets/Icons/GitHub_White.svg"
        : "/Assets/Icons/GitHub_Black.svg";

    public string SkyrimDataFolder
    {
        get => _settingsService.Settings.SkyrimDataFolder;
        set
        {
            _settingsService.Settings.SkyrimDataFolder = value;
            // TODO: Reload Mutagen Database when this changes
            // TODO: Reload Texture Packs when this changes
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
            // TODO: Scan for BodySlide presets when this changes
            OnPropertyChanged();
        }
    }

    public string RaceMenuPresetsFolder
    {
        get => _settingsService.Settings.RaceMenuPresetsFolder;
        set
        {
            _settingsService.Settings.RaceMenuPresetsFolder = value;
            // TODO: Scan for RaceMenu presets when this changes
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

    public int TexturePacksFound => _texturePackService.GetTexturePacks().Count;
    public int BodySlidePresetsFound => _bodySlideService.GetPresets().Count;

    public int RaceMenuPresetsFound => _raceMenuPresetService.GetPresets().Count;

    public double BaseFontSize
    {
        get => _settingsService.Settings.BaseFontSize;
        set
        {
            if (_settingsService.Settings.BaseFontSize == value)
                return;

            _settingsService.Settings.BaseFontSize = value;

            Application.Current!.Resources["FontSize"] = value;
            Application.Current.Resources["H1FontSize"] = value * 1.6;
            Application.Current.Resources["H2FontSize"] = value * 1.3;
            Application.Current.Resources["CaptionFontSize"] = value * 0.85;
            Application.Current.Resources["TinyFontSize"] = value * 0.7;

            OnPropertyChanged();
        }
    }

    public string Theme
    {
        get => _settingsService.Settings.Theme;
        set
        {
            if (_settingsService.Settings.Theme == value || Application.Current == null)
                return;

            _settingsService.Settings.Theme = value;
            Application.Current.RequestedThemeVariant = value switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
            OnPropertyChanged();
            OnPropertyChanged(nameof(GitHubIconPath)); 
        }
    }
}
