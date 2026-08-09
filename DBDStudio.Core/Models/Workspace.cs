using System.Collections.ObjectModel;
using DBDStudio.Core.Models.Textures;

namespace DBDStudio.Core.Models
{
    public sealed class Workspace
    {
        public ApplicationSettings Settings { get; } = new();
        public ObservableCollection<TexturePack> TexturePacks { get; } = [];
        public ObservableCollection<BodySlidePreset> BodySlidePresets { get; } = [];
        public ObservableCollection<RaceMenuPreset> RaceMenuPresets { get; } = [];
        public ObservableCollection<Rule> Rules { get; } = [];
    }
}
