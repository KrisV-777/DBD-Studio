using System.Collections.ObjectModel;
using DBDStudio.Models.Rules;
using DBDStudio.Models.Textures;

namespace DBDStudio.Models
{
    public sealed class Rule : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<TexturePack> TextureCandidates { get; } = [];
        public ObservableCollection<BodySlidePreset> BodySlideCandidates { get; } = [];
        public RaceMenuPreset? RaceMenuCandidates { get; set; } = null;
        public ObservableCollection<Condition> Conditions { get; } = [];
    }
}
