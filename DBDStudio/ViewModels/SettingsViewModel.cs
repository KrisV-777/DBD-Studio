using System.Windows.Input;
using DBDStudio.Interfaces;
using System.Diagnostics;
using Avalonia;
using Avalonia.Styling;
using DBDStudio.Models;

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
            _bodySlideService.Presets.CollectionChanged += (_, _) => OnPropertyChanged(nameof(BodySlidePresetsFound));
            _raceMenuPresetService.Presets.CollectionChanged += (_, _) => OnPropertyChanged(nameof(RaceMenuPresetsFound));

            var openUrl = (string url) => Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
            CmdOpenGithub = new RelayCommand(() => openUrl("https://github.com/"));
            CmdOpenWiki = new RelayCommand(() => openUrl("https://github.com/wiki"));
            CmdOpenNexus = new RelayCommand(() => openUrl("https://www.nexusmods.com/"));
            CmdOpenKofi = new RelayCommand(() => openUrl("https://ko-fi.com/"));
        }

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
                Debug.Assert(!string.IsNullOrWhiteSpace(SkyrimDataFolder));
                _appSettings.BodySlidePresetsFolder = ValidatePresetsFolder(value);
                OnPropertyChanged();
            }
        }

        public string RaceMenuPresetsFolder
        {
            get => _appSettings.RaceMenuPresetsFolder;
            set
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(SkyrimDataFolder));
                _appSettings.RaceMenuPresetsFolder = ValidatePresetsFolder(value);
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

        public int TexturePacksFound => _texturePackService.TexturePacks.Count;
        public int BodySlidePresetsFound => _bodySlideService.Presets.Count;

        public int RaceMenuPresetsFound => _raceMenuPresetService.Presets.Count;

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

        private string ValidatePresetsFolder(string value)
        {
            if (!Path.IsPathFullyQualified(value)) {
                return value;
            }

            var fullPath = Path.GetFullPath(value);
            var skyrimData = Path.GetFullPath(SkyrimDataFolder);
            var modsFolder = Path.GetFullPath(ModsFolder);

            if (IsChildPath(skyrimData, fullPath)) {
                return Path.GetRelativePath(skyrimData, fullPath);
            }

            if (IsChildPath(modsFolder, fullPath)) {
                var relativeToMods = Path.GetRelativePath(modsFolder, fullPath);

                // Remove the mod folder itself:
                // Mods\SomeMod\Presets\Sliders -> Presets\Sliders
                var separator = relativeToMods.IndexOfAny(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);

                return separator >= 0
                        ? relativeToMods[(separator + 1)..]
                        : string.Empty;
            }

            // Absolute path, but not under either known folder.
            return value;
        }

        private static bool IsChildPath(string parent, string child)
        {
            var relative = Path.GetRelativePath(parent, child);

            return relative != ".."
                && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !Path.IsPathRooted(relative);
        }
    }
}
