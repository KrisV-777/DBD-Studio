using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using System.Diagnostics;
using Avalonia;
using Avalonia.Styling;
using DBDStudio.Core.Models;

namespace DBDStudio.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private readonly ApplicationSettings _appSettings;
        private readonly ITexturePackService _texturePackService;
        private readonly IBodySlideService _bodySlideService;
        private readonly IRaceMenuPresetService _raceMenuPresetService;
        private readonly MainWindowViewModel? _mainWindowViewModel;

        public static readonly string[] ThemeOptions = ["Light", "Dark", "System"];

        public SettingsViewModel(
            ApplicationSettings settingsService,
            ITexturePackService texturePackService,
            IBodySlideService bodySlideService,
            IRaceMenuPresetService raceMenuPresetService,
            MainWindowViewModel? mainWindowViewModel = null)
        {
            _appSettings = settingsService;
            _texturePackService = texturePackService;
            _bodySlideService = bodySlideService;
            _raceMenuPresetService = raceMenuPresetService;
            _mainWindowViewModel = mainWindowViewModel;

            _appSettings.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
            _texturePackService.TexturePackListChanged += (_, _) => OnPropertyChanged(nameof(TexturePacksFound));

            SaveCommand = new RelayCommand(SaveSettings);
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

        private void SaveSettings()
        {
            _texturePackService.ResetTextureList();
            OnPropertyChanged(nameof(TexturePacksFound));
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
            get => _appSettings.SkyrimDataFolder;
            set
            {
                _appSettings.SkyrimDataFolder = value;
                // TODO: Reload Mutagen Database when this changes
                // TODO: Reload Texture Packs when this changes
                OnPropertyChanged();
            }
        }

        public string ModsFolder
        {
            get => _appSettings.ModsFolder;
            set
            {
                _appSettings.ModsFolder = value;
                OnPropertyChanged();
            }
        }

        public string BodySlidePresetsFolder
        {
            get => _appSettings.BodySlidePresetsFolder;
            set
            {
                _appSettings.BodySlidePresetsFolder = value;
                // TODO: Scan for BodySlide presets when this changes
                OnPropertyChanged();
            }
        }

        public string RaceMenuPresetsFolder
        {
            get => _appSettings.RaceMenuPresetsFolder;
            set
            {
                _appSettings.RaceMenuPresetsFolder = value;
                // TODO: Scan for RaceMenu presets when this changes
                OnPropertyChanged();
            }
        }

        public static string BodySlidePresetsFolderDefault => ApplicationSettings.BodySlidePresetsFolderDefault;
        public static string RaceMenuPresetsFolderDefault => ApplicationSettings.RaceMenuPresetsFolderDefault;

        public string WorkspaceFilePath
        {
            get => _appSettings.WorkspaceFilePath;
            set
            {
                _appSettings.WorkspaceFilePath = value;
                OnPropertyChanged();
            }
        }

        public int TexturePacksFound => _texturePackService.GetTexturePacks().Count;
        public int BodySlidePresetsFound => _bodySlideService.GetPresets().Count;

        public int RaceMenuPresetsFound => _raceMenuPresetService.GetPresets().Count;

        public double BaseFontSize
        {
            get => _appSettings.BaseFontSize;
            set
            {
                if (_appSettings.BaseFontSize == value)
                    return;

                _appSettings.BaseFontSize = value;

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
            get => _appSettings.Theme;
            set
            {
                if (_appSettings.Theme == value || Application.Current == null)
                    return;

                _appSettings.Theme = value;
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
}
