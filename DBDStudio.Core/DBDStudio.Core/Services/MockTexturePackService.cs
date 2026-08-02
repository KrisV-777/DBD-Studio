using System.Collections.Generic;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockTexturePackService : ITexturePackService
{
    private readonly List<TexturePack> _packs =
    [
        new() { Name = "Fair Skin", Description = "A broad, bright skin option for female NPCs.", Visibility = TexturePackVisibility.Public, RandomPool = true },
        new() { Name = "Bijin", Description = "High-contrast character textures.", Visibility = TexturePackVisibility.Public, RandomPool = false },
        new() { Name = "Tempered", Description = "Balanced textures for a clean look.", Visibility = TexturePackVisibility.Private, RandomPool = false },
        new() { Name = "Player HD", Description = "A high-detail player texture set.", Visibility = TexturePackVisibility.Public, RandomPool = true }
    ];

    public IReadOnlyList<TexturePack> GetTexturePacks() => _packs;

    public void Add(TexturePack pack) => _packs.Add(pack);

    public void Update(TexturePack pack)
    {
        var index = _packs.FindIndex(x => x.Name == pack.Name);
        if (index >= 0)
            _packs[index] = pack;
    }

    public void Remove(TexturePack pack) => _packs.Remove(pack);
}
