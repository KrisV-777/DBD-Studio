using System.Collections.ObjectModel;

namespace DBDStudio.Models
{
    public sealed class Rule
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public ObservableCollection<string> TextureCandidates { get; } = [];
        public ObservableCollection<string> BodySlideCandidates { get; } = [];
        public ObservableCollection<string> RaceMenuCandidates { get; } = [];

        public string TexturePack
        {
            get => TextureCandidates.Count > 0 ? TextureCandidates[0] : string.Empty;
            set => SetSingleCandidate(TextureCandidates, value);
        }

        public string BodySlidePreset
        {
            get => BodySlideCandidates.Count > 0 ? BodySlideCandidates[0] : string.Empty;
            set => SetSingleCandidate(BodySlideCandidates, value);
        }

        public string RaceMenuPreset
        {
            get => RaceMenuCandidates.Count > 0 ? RaceMenuCandidates[0] : string.Empty;
            set => SetSingleCandidate(RaceMenuCandidates, value);
        }

        public string PriorityPreview { get; set; } = "Generic Match";
        public ObservableCollection<Condition> Conditions { get; } = [];

        private static void SetSingleCandidate(ObservableCollection<string> candidates, string value)
        {
            candidates.Clear();
            if (!string.IsNullOrWhiteSpace(value)) {
                candidates.Add(value);
            }
        }
    }
}
