using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IWorkspaceService
    {
        Workspace Current { get; }
        void Load();
        void Save();
    }
}
