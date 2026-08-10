using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBDStudio.Core.Models
{
    public sealed class ApplicationSettings : INotifyPropertyChanged
    {
        private string _workspaceFilePath = BuildDefaultWorkspacePath();
        private string _skyrimDataFolder = string.Empty;
        private string _modsFolder = string.Empty;
        private string _bodySlidePresetsFolder = string.Empty;
        private string _raceMenuPresetsFolder = string.Empty;
        private double _baseFontSize = 14;
        private string _theme = "System";

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
