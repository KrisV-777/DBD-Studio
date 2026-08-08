using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services
{
    public sealed class MockSettingsService : ISettingsService
    {
        private readonly IWorkspaceService _workspaceService;

        public MockSettingsService(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        public ApplicationSettings Settings => _workspaceService.Current.Settings;

        public void Load()
        {
            _workspaceService.Load();
        }

        public void Save()
        {
            _workspaceService.Save();
        }
    }
}
