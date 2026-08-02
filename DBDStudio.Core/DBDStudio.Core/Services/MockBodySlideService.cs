using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockBodySlideService : IBodySlideService
{
    private readonly List<BodySlidePreset> _presets =
    [
        new() { Preset = "CBBE Curvy", SourceXml = "CBBE.xml" },
        new() { Preset = "BHUNP Slim", SourceXml = "BHUNP.xml" },
        new() { Preset = "UUNP Special", SourceXml = "UUNP.xml" }
    ];

    public IReadOnlyList<BodySlidePreset> GetPresets() => _presets;
}
