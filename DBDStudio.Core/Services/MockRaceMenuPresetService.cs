using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services
{
    public sealed class MockRaceMenuPresetService : IRaceMenuPresetService
    {
        private readonly IWorkspaceService _workspaceService;

        public MockRaceMenuPresetService(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        public IReadOnlyList<RaceMenuPreset> GetPresets() => _workspaceService.Current.RaceMenuPresets;

        public void Add(RaceMenuPreset preset) => _workspaceService.Current.RaceMenuPresets.Add(preset);

        public void Update(RaceMenuPreset preset)
        {
            var existing = _workspaceService.Current.RaceMenuPresets.FirstOrDefault(x => x.Name == preset.Name);
            if (existing is null) {
                return;
            }

            existing.JsSlotFile = preset.JsSlotFile;
            existing.Sex = preset.Sex;
            existing.NifFile = preset.NifFile;
            existing.DdsFile = preset.DdsFile;
        }

        public void Remove(RaceMenuPreset preset) => _workspaceService.Current.RaceMenuPresets.Remove(preset);
    }
}
