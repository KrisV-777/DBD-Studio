using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Persistence;
using Noggog;
using System.Reactive.Linq;

namespace DBDStudio.Core.Services
{
    public sealed class BodySlideService : IBodySlideService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        private readonly ObservableCollection<BodySlidePreset> _presets = [];

        #region Constructor

        public BodySlideService(ApplicationSettings settings)
        {
            _settings = settings;

            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or 
                    nameof(ApplicationSettings.ModsFolder) or nameof(ApplicationSettings.BodySlidePresetsFolder)) {
                    Reset();
                }
            };
        }

        #endregion

        #region IBodySlideService

        public ObservableCollection<BodySlidePreset> Presets => _presets;

        public void Reset() => ReInitializePresets(
            DiscoverExternalPresets(_settings.SkyrimDataFolder).Union(DiscoverExternalPresets(_settings.ModsFolder)));
    
        #endregion

        #region IPersistable

        public string PersistenceKey => "bodySlidePresets";
        public Type PersistenceStateType => typeof(List<BodySlidePreset>);

        public object? SaveState() => _presets;

        public void RestoreState(object? state)
        {
            if (state is not List<BodySlidePreset> savedPresets) {
                return;
            }
            ReInitializePresets(savedPresets);
        }

        #endregion

        #region Private Methods

        private void ReInitializePresets(IEnumerable<BodySlidePreset> newPresets)
        {
            var oldPresets = _presets.ToHashSet();
            _presets.Clear();

            newPresets
                .Where(p => !string.IsNullOrWhiteSpace(p.Preset) && File.Exists(p.SourceXml))
                .OrderBy(p => p.Preset, StringComparer.OrdinalIgnoreCase)
                .ForEach(p =>
                {
                    if (oldPresets.TryGetValue(p, out var old))
                        p.IsPrivate = old.IsPrivate;

                    _presets.Add(p);
                });
        }

        private IEnumerable<BodySlidePreset> DiscoverExternalPresets(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder)) {
                yield break;
            }

            foreach (var xmlFileInfo in EnumerateConfigFiles(rootFolder)) {
                var xmlFile = xmlFileInfo.FullName;
                var document = XDocument.Load(xmlFile);

                foreach (var element in document.Root?.Elements("Preset") ?? []) {
                    yield return new BodySlidePreset {
                        Preset = (string?)element.Attribute("name") ?? string.Empty,
                        Group = (string?)element.Element("Group")?.Attribute("name") ?? string.Empty,
                        SourceXml = xmlFile,
                        IsPrivate = false
                    };
                }
            }
        }

        private IEnumerable<FileInfo> EnumerateConfigFiles(string rootFolder)
        {
            // Pattern 1: rootFolder/<config.bodyslidePath>/*.xml
            var sliderPresets = Path.Combine(rootFolder, _settings.BodySlidePresetsFolder);
            if (Directory.Exists(sliderPresets)) {
                var xmlFiles = Directory.EnumerateFiles(sliderPresets, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (var xmlFile in xmlFiles) {
                    yield return new FileInfo(xmlFile);
                }
            }
            // Pattern 2: rootFolder/*/<config.bodyslidePath>/*.xml
            foreach (var subdir in Directory.EnumerateDirectories(rootFolder)) {
                var sliderPresetsSub = Path.Combine(subdir, _settings.BodySlidePresetsFolder);
                if (!Directory.Exists(sliderPresetsSub))
                    continue;
                var xmlFiles = Directory.EnumerateFiles(sliderPresetsSub, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (var xmlFile in xmlFiles) {
                    yield return new FileInfo(xmlFile);
                }
            }
        }

        #endregion
    }
}
