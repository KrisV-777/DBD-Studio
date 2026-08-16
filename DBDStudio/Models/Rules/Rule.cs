using System.Collections.ObjectModel;
using DBDStudio.Models.Rules;

namespace DBDStudio.Models
{
    public sealed class Rule : ModelBase
    {
        public string Name { get; set; } = string.Empty;
        public string PriorityPreview { get; set; } = string.Empty;
        public ObservableCollection<string> TextureCandidates { get; } = [];
        public ObservableCollection<string> BodySlideCandidates { get; } = [];
        public string? RaceMenuCandidate { get; set; }
        public ObservableCollection<Condition> Conditions { get; } = [];

        public Rule DeepClone()
        {
            var clone = new Rule {
                Name = Name,
                PriorityPreview = PriorityPreview,
                RaceMenuCandidate = RaceMenuCandidate
            };

            foreach (var candidate in TextureCandidates)
                clone.TextureCandidates.Add(candidate);

            foreach (var candidate in BodySlideCandidates)
                clone.BodySlideCandidates.Add(candidate);

            foreach (var condition in Conditions)
                clone.Conditions.Add(condition.DeepClone());

            return clone;
        }
    }
}
