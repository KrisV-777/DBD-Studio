using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public enum TexturePackState
    {
        None = -1,
        /// <summary>The texture pack only exists in memory and has no representation outside of the application.</summary>
        Ephemeral,
        /// <summary>The texture pack has been loaded from a file on disk and has not been modified since it was loaded.</summary>
        Disk,
        /// <summary>The texture pack has been loaded from a file on disk and has been modified since it was loaded.</summary>
        DiskEdited
    }

    public interface ITexturePackService
    {
        event EventHandler? TexturePacksChanged;
        IReadOnlyList<TexturePack> GetTexturePacks();

        void RefreshFromConfiguredFolders();
        void Add(TexturePack pack);
        void TryAdd(TexturePack pack);
        void Remove(TexturePack pack);

        TexturePackState GetTexturePackState(TexturePack pack);
        void ResetToDiskState(TexturePack pack);
    }
}
