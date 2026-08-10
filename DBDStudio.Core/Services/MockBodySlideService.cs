using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Persistence;

namespace DBDStudio.Core.Services
{
    public sealed class MockBodySlideService : IBodySlideService, IPersistable
    {
        private readonly List<BodySlidePreset> _presets = [];

        public string PersistenceKey => "bodySlidePresets";
        public Type PersistenceStateType => typeof(BodySlidePresetPersistenceState);

        public object? SaveState()
        {
            return new BodySlidePresetPersistenceState {
                Presets = [.. _presets.Select(preset => new BodySlidePreset {
                    Preset = preset.Preset,
                    SourceXml = preset.SourceXml
                })]
            };
        }

        public void RestoreState(object? state)
        {
            _presets.Clear();
            if (state is not BodySlidePresetPersistenceState persistenceState) {
                return;
            }

            foreach (var preset in persistenceState.Presets) {
                _presets.Add(new BodySlidePreset {
                    Preset = preset.Preset,
                    SourceXml = preset.SourceXml
                });
            }
        }

        public IReadOnlyList<BodySlidePreset> GetPresets() => _presets;
    }
}
