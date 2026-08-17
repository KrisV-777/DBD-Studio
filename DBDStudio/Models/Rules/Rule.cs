using System.Collections.ObjectModel;
using DBDStudio.Models.Rules;
using System.Text.Json.Serialization;

namespace DBDStudio.Models
{
    public sealed class Rule : ModelBase
    {
        private string _name = string.Empty;
        private string? _raceMenuCandidate = null;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string PriorityPreview { get; set; } = string.Empty; // TODO: Delete this <

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public ObservableCollection<string> TextureCandidates { get; } = [];

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public ObservableCollection<string> BodySlideCandidates { get; } = [];

        public string? RaceMenuCandidate
        {
            get => _raceMenuCandidate;
            set => SetProperty(ref _raceMenuCandidate, value);
        }

        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
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
