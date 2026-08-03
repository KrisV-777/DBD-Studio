using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockBodySlideService : IBodySlideService
{
    private readonly IWorkspaceService _workspaceService;

    public MockBodySlideService(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public IReadOnlyList<BodySlidePreset> GetPresets() => _workspaceService.Current.BodySlidePresets;
}
