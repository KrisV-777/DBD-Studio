using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockRaceMenuPresetService : IRaceMenuPresetService
{
    private readonly List<RaceMenuPreset> _presets =
    [
        new()
        {
            Name = "LydiaPreset",
            JsSlotFile = "LydiaPreset.jslot",
            Sex = "Female",
            NifFile = "LydiaPreset.nif",
            DdsFile = "LydiaPreset.dds"
        },
        new()
        {
            Name = "WarriorMale",
            JsSlotFile = "WarriorMale.jslot",
            Sex = "Male",
            NifFile = "WarriorMale.nif"
        },
        new()
        {
            Name = "CustomFemale",
            JsSlotFile = "CustomFemale.jslot",
            Sex = "Female"
        }
    ];

    public IReadOnlyList<RaceMenuPreset> GetPresets() => _presets;

    public void Add(RaceMenuPreset preset) => _presets.Add(preset);

    public void Update(RaceMenuPreset preset)
    {
        var index = _presets.FindIndex(x => x.Name == preset.Name);
        if (index >= 0)
            _presets[index] = preset;
    }

    public void Remove(RaceMenuPreset preset) => _presets.Remove(preset);
}
