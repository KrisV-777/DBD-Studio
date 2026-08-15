using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Utility;
using Noggog;

namespace DBDStudio.Services
{
    public sealed class RaceMenuPresetService : IRaceMenuPresetService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        private readonly ObservableCollection<RaceMenuPreset> _presets = [];

        public RaceMenuPresetService(ApplicationSettings settings)
        {
            _settings = settings;

            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or
                    nameof(ApplicationSettings.ModsFolder) or nameof(ApplicationSettings.RaceMenuPresetsFolder)) {
                    Reset();
                }
            };
        }

        public string PersistenceKey => "raceMenuPresets";
        public Type PersistenceStateType => typeof(List<RaceMenuPreset>);

        public ObservableCollection<RaceMenuPreset> Presets => _presets;

        public void Reset() => ReInitializePresets(
            DiscoverExternalPresets(_settings.SkyrimDataFolder).Union(DiscoverExternalPresets(_settings.ModsFolder)));

        public object? SaveState() => _presets;

        public void RestoreState(object? state)
        {
            if (state is not List<RaceMenuPreset> savedPresets) {
                return;
            }
            ReInitializePresets(savedPresets);
        }

        private void ReInitializePresets(IEnumerable<RaceMenuPreset> newPresets)
        {
            var oldPresets = _presets.ToHashSet();
            _presets.Clear();

            newPresets
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && File.Exists(p.JsSlotFile))
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ForEach(p =>
                {
                    if (oldPresets.TryGetValue(p, out var old) && IsValidSex(old.Sex))
                        p.Sex = old.Sex;

                    _presets.Add(p);
                });
        }

        private IEnumerable<RaceMenuPreset> DiscoverExternalPresets(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder)) {
                yield break;
            }

            foreach (var presetFile in DirectoryIterator.EnumerateProjectFiles([
                    new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                    new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                ], _settings.RaceMenuPresetsFolder, "*.jslot")) {
                var presetFilePath = presetFile.FullName;
                var sex = InferSexFromPathOrName(presetFilePath);

                yield return new RaceMenuPreset {
                    Name = presetFile.Name,
                    JsSlotFile = presetFilePath,
                    Sex = sex
                };
            }
        }

        private static string InferSexFromPathOrName(string presetFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(presetFile);
            var filePath = presetFile;

            if (ContainsMarker(fileName, "female") || ContainsMarker(filePath, "female") ||
                ContainsMarker(fileName, "fem") || ContainsMarker(filePath, "fem")) {
                return "Female";
            }

            if (ContainsMarker(fileName, "male") || ContainsMarker(filePath, "male") ||
                ContainsMarker(fileName, "masc") || ContainsMarker(filePath, "masc")) {
                return "Male";
            }

            return "Male";
        }

        private static bool ContainsMarker(string source, string marker)
            => source.Contains(marker, StringComparison.OrdinalIgnoreCase);

        private static bool IsValidSex(string sex)
            => string.Equals(sex, "Male", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sex, "Female", StringComparison.OrdinalIgnoreCase);
    }
}
