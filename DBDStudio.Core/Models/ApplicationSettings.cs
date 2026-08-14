using System.ComponentModel;
using System.Runtime.CompilerServices;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Utility.Persistence;

namespace DBDStudio.Core.Models
{
    public sealed class ApplicationSettings : INotifyPropertyChanged, IPersistable
    {
        private string _workspaceFilePath = BuildDefaultWorkspacePath();
        private string _skyrimDataFolder = string.Empty;
        private string _modsFolder = string.Empty;
        private string _bodySlidePresetsFolder = BodySlidePresetsFolderDefault;
        private string _raceMenuPresetsFolder = RaceMenuPresetsFolderDefault;
        private double _baseFontSize = 14;
        private string _theme = "System";

        public string PersistenceKey => "settings";
        public Type PersistenceStateType => typeof(ApplicationSettingsPersistenceState);

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

        public static string BodySlidePresetsFolderDefault => "CalienteTools\\BodySlide\\SliderPresets";
        public static string RaceMenuPresetsFolderDefault => "SKSE\\Plugins\\CharGen\\Presets";

        public double BaseFontSize
        {
            get => _baseFontSize;
            set => SetProperty(ref _baseFontSize, value);
        }

        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? SaveState()
        {
            return new ApplicationSettingsPersistenceState {
                WorkspaceFilePath = _workspaceFilePath,
                SkyrimDataFolder = _skyrimDataFolder,
                ModsFolder = _modsFolder,
                BodySlidePresetsFolder = _bodySlidePresetsFolder,
                RaceMenuPresetsFolder = _raceMenuPresetsFolder,
                BaseFontSize = _baseFontSize,
                Theme = _theme
            };
        }

        public void RestoreState(object? state)
        {
            if (state is not ApplicationSettingsPersistenceState settings) {
                return;
            }

            WorkspaceFilePath = settings.WorkspaceFilePath;
            SkyrimDataFolder = settings.SkyrimDataFolder;
            ModsFolder = settings.ModsFolder;
            BodySlidePresetsFolder = settings.BodySlidePresetsFolder;
            RaceMenuPresetsFolder = settings.RaceMenuPresetsFolder;
            BaseFontSize = settings.BaseFontSize;
            Theme = settings.Theme;
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string BuildDefaultWorkspacePath()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataFolder, "DBDStudio", "workspace.dbdproj");
        }
    }
}
