using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface ISettingsService
    {
        ApplicationSettings Settings { get; }
        void Load();
        void Save();
    }
}
