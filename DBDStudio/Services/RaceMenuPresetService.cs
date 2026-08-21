using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Utility;
using Noggog;

namespace DBDStudio.Services
{
    public sealed class RaceMenuPresetService : IRaceMenuPresetService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        public ObservableCollection<RaceMenuPreset> Presets { get; } = [];

        public RaceMenuPresetService(ApplicationSettings settings)
        {
            _settings = settings;

            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or
                    nameof(ApplicationSettings.RaceMenuPresetsFolder) or
                    nameof(ApplicationSettings.ModsFolder)) {
                    ReInitializePresets(DiscoverExternalPresets());
                }
            };
        }

        private void ReInitializePresets(IEnumerable<RaceMenuPreset> newPresets)
        {
            Presets.Clear();

            newPresets
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && File.Exists(p.JslotFile))
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ForEach(p => Presets.Add(p));
        }

        private IEnumerable<RaceMenuPreset> DiscoverExternalPresets()
        {
            foreach (var presetFile in DirectoryIterator.EnumerateProjectFiles([
                    new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                    new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                ], _settings.RaceMenuPresetsFolder, "*.jslot")) {
                yield return new RaceMenuPreset {
                    Name = presetFile.Name,
                    JslotFile = presetFile.FullName,
                };
            }
        }

        #region IPersistable

        public string PersistenceKey => "raceMenuPresets";
        public Type PersistenceStateType => typeof(List<RaceMenuPreset>);

        public object? SaveState() => Presets;

        public void RestoreState(object? state)
        {
            if (state is not List<RaceMenuPreset> savedPresets) {
                return;
            }
            ReInitializePresets(savedPresets);
        }

        #endregion
    }
}
