using System.Collections.ObjectModel;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IBodySlideService
    {
        ObservableCollection<BodySlidePreset> Presets { get; }
        void Reset();
    }
}
