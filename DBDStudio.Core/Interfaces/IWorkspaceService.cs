using DBDStudio.Core.Models;
using DBDStudio.Core.Models.Textures;

namespace DBDStudio.Core.Interfaces
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
