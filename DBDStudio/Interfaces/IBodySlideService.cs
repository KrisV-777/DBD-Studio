using System.Collections.ObjectModel;
using DBDStudio.Models;

namespace DBDStudio.Interfaces
{
    public interface IBodySlideService
    {
        ObservableCollection<BodySlidePreset> Presets { get; }
        void Reset();
    }
}
