using DBDStudio.Models;
using DBDStudio.Models.Textures;

namespace DBDStudio.Interfaces
{
    public interface IWorkspaceService
    {
        ApplicationSettings Settings { get; }
        IReadOnlyList<TexturePack> TexturePacks { get; }
        IReadOnlyList<BodySlidePreset> BodySlidePresets { get; }
        IReadOnlyList<RaceMenuPreset> RaceMenuPresets { get; }
        IReadOnlyList<Rule> Rules { get; }

        void Load();
        void Save();
    }
}
