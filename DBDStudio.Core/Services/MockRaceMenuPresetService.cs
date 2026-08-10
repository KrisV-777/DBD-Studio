using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Persistence;

namespace DBDStudio.Core.Services
{
    public sealed class MockRaceMenuPresetService : IRaceMenuPresetService, IPersistable
    {
        private readonly List<RaceMenuPreset> _presets = [];

        public string PersistenceKey => "raceMenuPresets";
        public Type PersistenceStateType => typeof(RaceMenuPresetPersistenceState);

        public object? SaveState()
        {
            return new RaceMenuPresetPersistenceState {
                Presets = [.. _presets.Select(preset => new RaceMenuPreset {
                    Name = preset.Name,
                    JsSlotFile = preset.JsSlotFile,
                    Sex = preset.Sex,
                    NifFile = preset.NifFile,
                    DdsFile = preset.DdsFile
                })]
            };
        }

        public void RestoreState(object? state)
        {
            _presets.Clear();
            if (state is not RaceMenuPresetPersistenceState persistenceState) {
                return;
            }

            foreach (var preset in persistenceState.Presets) {
                _presets.Add(new RaceMenuPreset {
                    Name = preset.Name,
                    JsSlotFile = preset.JsSlotFile,
                    Sex = preset.Sex,
                    NifFile = preset.NifFile,
                    DdsFile = preset.DdsFile
                });
            }
        }

        public IReadOnlyList<RaceMenuPreset> GetPresets() => _presets;

        public void Add(RaceMenuPreset preset) => _presets.Add(new RaceMenuPreset {
            Name = preset.Name,
            JsSlotFile = preset.JsSlotFile,
            Sex = preset.Sex,
            NifFile = preset.NifFile,
            DdsFile = preset.DdsFile
        });

        public void Update(RaceMenuPreset preset)
        {
            var existing = _presets.FirstOrDefault(x => x.Name == preset.Name);
            if (existing is null) {
                return;
            }

            existing.JsSlotFile = preset.JsSlotFile;
            existing.Sex = preset.Sex;
            existing.NifFile = preset.NifFile;
            existing.DdsFile = preset.DdsFile;
        }

        public void Remove(RaceMenuPreset preset)
        {
            var existing = _presets.FirstOrDefault(x => x.Name == preset.Name);
            if (existing is not null) {
                _presets.Remove(existing);
            }
        }
    }
}
