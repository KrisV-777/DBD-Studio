using System.Collections.Generic;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces;

public interface IBodySlideService
{
    IReadOnlyList<BodySlidePreset> GetPresets();
}
