using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Utility.Persistence;
using Noggog;
using System.Reactive.Linq;
using DBDStudio.Core.Utility;

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

        public void Reset() => ReInitializePresets(DiscoverExternalPresets());

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

        private IEnumerable<BodySlidePreset> DiscoverExternalPresets()
        {
            foreach (var xmlFileInfo in DirectoryIterator.EnumerateProjectFiles([
                    new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                    new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                ], _settings.BodySlidePresetsFolder, "*.xml")) {
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

        #endregion
    }
}
