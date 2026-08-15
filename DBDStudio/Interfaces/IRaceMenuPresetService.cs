using System.Collections.ObjectModel;
using DBDStudio.Models;

namespace DBDStudio.Interfaces
{
    public interface IRaceMenuPresetService
    {
        ObservableCollection<RaceMenuPreset> Presets { get; }
        void Reset();
    }
}
