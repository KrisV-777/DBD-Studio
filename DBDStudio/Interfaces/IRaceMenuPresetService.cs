using System.Collections.ObjectModel;
using DBDStudio.Models.Component;

namespace DBDStudio.Interfaces
{
    public interface IRaceMenuPresetService
    {
        ObservableCollection<RaceMenuPreset> Presets { get; }
    }
}
