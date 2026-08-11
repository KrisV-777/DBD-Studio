using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Persistence;
using Noggog;

namespace DBDStudio.Core.Services
{
    public sealed class MockRaceMenuPresetService : IRaceMenuPresetService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        private readonly ObservableCollection<RaceMenuPreset> _presets = [];

        public MockRaceMenuPresetService(ApplicationSettings settings)
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

            foreach (var presetFile in EnumeratePresetFiles(rootFolder)) {
                var sex = InferSexFromPathOrName(presetFile);

                yield return new RaceMenuPreset {
                    Name = Path.GetFileNameWithoutExtension(presetFile),
                    JsSlotFile = presetFile,
                    Sex = sex
                };
            }
        }

        private IEnumerable<string> EnumeratePresetFiles(string rootFolder)
        {
            // Pattern 1: rootFolder/<config.raceMenuPath>/*.jslot
            var presetsFolder = Path.Combine(rootFolder, _settings.RaceMenuPresetsFolder);
            if (Directory.Exists(presetsFolder)) {
                foreach (var presetFile in Directory.EnumerateFiles(presetsFolder, "*.jslot", SearchOption.TopDirectoryOnly)) {
                    yield return presetFile;
                }
            }

            // Pattern 2: rootFolder/*/<config.raceMenuPath>/*.jslot
            foreach (var subdir in Directory.EnumerateDirectories(rootFolder)) {
                var presetsFolderSub = Path.Combine(subdir, _settings.RaceMenuPresetsFolder);
                if (!Directory.Exists(presetsFolderSub))
                    continue;

                foreach (var presetFile in Directory.EnumerateFiles(presetsFolderSub, "*.jslot", SearchOption.TopDirectoryOnly)) {
                    yield return presetFile;
                }
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
