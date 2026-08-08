using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface ITexturePackService
    {
        event EventHandler? TexturePacksChanged;

        IReadOnlyList<TexturePack> GetTexturePacks();
        void RefreshFromConfiguredFolders();
        void Add(TexturePack pack);
        void Remove(TexturePack pack);
    }
}
