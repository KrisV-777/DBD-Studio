using System.Collections.ObjectModel;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IRaceMenuPresetService
    {
        ObservableCollection<RaceMenuPreset> Presets { get; }
        void Reset();
    }
}
