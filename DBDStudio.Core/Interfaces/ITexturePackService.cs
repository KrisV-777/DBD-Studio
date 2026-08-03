using System.Collections.Generic;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces;

public interface ITexturePackService
{
    IReadOnlyList<TexturePack> GetTexturePacks();
    void Add(TexturePack pack);
    void Update(TexturePack pack);
    void Remove(TexturePack pack);
}
