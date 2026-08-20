using System.Collections.ObjectModel;
using DBDStudio.Models.Component;

namespace DBDStudio.Interfaces
{
    public interface IBodySlideService
    {
        ObservableCollection<BodySlidePreset> Presets { get; }
    }
}
