using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using Noggog;
using System.Reactive.Linq;
using DBDStudio.Utility;
using DBDStudio.Models.Component;

namespace DBDStudio.Services
{
    public sealed class BodySlideService : IBodySlideService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        public ObservableCollection<BodySlidePreset> Presets { get; } = [];

        public BodySlideService(ApplicationSettings settings)
        {
            _settings = settings;

            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or
                    nameof(ApplicationSettings.BodySlidePresetsFolder) or
                    nameof(ApplicationSettings.ModsFolder)) {
                    ReInitializePresets(DiscoverExternalPresets());
                }
            };
        }

        #region Private Methods

        private void ReInitializePresets(IEnumerable<BodySlidePreset> newPresets)
        {
            var oldPresets = Presets.ToHashSet();
            Presets.Clear();

            newPresets
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && File.Exists(p.SourceXml))
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ForEach(p =>
                {
                    if (oldPresets.TryGetValue(p, out var old))
                        p.IsPrivate = old.IsPrivate;

                    Presets.Add(p);
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
                        Name = (string?)element.Attribute("name") ?? string.Empty,
                        SourceXml = xmlFile,
                        IsPrivate = false
                    };
                }
            }
        }

        #endregion

        #region IPersistable

        public string PersistenceKey => "bodySlidePresets";
        public Type PersistenceStateType => typeof(List<BodySlidePreset>);

        public object? SaveState() => Presets;

        public void RestoreState(object? state)
        {
            if (state is not List<BodySlidePreset> savedPresets) {
                return;
            }
            ReInitializePresets(savedPresets);
        }

        #endregion
    }
}
