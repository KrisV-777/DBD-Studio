using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockTexturePackService : ITexturePackService
{
    private readonly IWorkspaceService _workspaceService;

    public MockTexturePackService(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    public IReadOnlyList<TexturePack> GetTexturePacks() => _workspaceService.Current.TexturePacks;

    public void Add(TexturePack pack) => _workspaceService.Current.TexturePacks.Add(pack);

    public void Update(TexturePack pack)
    {
        var existing = _workspaceService.Current.TexturePacks.FirstOrDefault(x => x.Name == pack.Name);
        if (existing is null)
            return;

        existing.Description = pack.Description;
        existing.Visibility = pack.Visibility;
        existing.RandomPool = pack.RandomPool;
        existing.Mappings.Clear();
        foreach (var mapping in pack.Mappings)
            existing.Mappings.Add(mapping);
    }

    public void Remove(TexturePack pack) => _workspaceService.Current.TexturePacks.Remove(pack);
}
