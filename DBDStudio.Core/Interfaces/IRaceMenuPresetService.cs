using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IRaceMenuPresetService
    {
        IReadOnlyList<RaceMenuPreset> GetPresets();
        void Add(RaceMenuPreset preset);
        void Update(RaceMenuPreset preset);
        void Remove(RaceMenuPreset preset);
    }
}
