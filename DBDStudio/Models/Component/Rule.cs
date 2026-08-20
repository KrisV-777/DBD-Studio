using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Models.Component
{
    public sealed class Rule : DBDComponent
    {
        private string? _raceMenuCandidate = null;

        #region Properties

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
        public ObservableCollection<ICondition> Conditions { get; } = [];

        #endregion

        #region Constructors

        internal override DBDComponent Copy()
        {
            var clone = new Rule {
                Name = Name,
                RaceMenuCandidate = RaceMenuCandidate
            };

            foreach (var candidate in TextureCandidates)
                clone.TextureCandidates.Add(candidate);

            foreach (var candidate in BodySlideCandidates)
                clone.BodySlideCandidates.Add(candidate);

            foreach (var condition in Conditions)
                clone.Conditions.Add(condition.Copy());

            return clone;
        }

        internal override void Import(DBDComponent source)
        {
            if (source is not Rule sourceRule)
                throw new ArgumentException("Source must be of type Rule.", nameof(source));

            Name = sourceRule.Name;
            RaceMenuCandidate = sourceRule.RaceMenuCandidate;

            TextureCandidates.Clear();
            foreach (var candidate in sourceRule.TextureCandidates)
                TextureCandidates.Add(candidate);

            BodySlideCandidates.Clear();
            foreach (var candidate in sourceRule.BodySlideCandidates)
                BodySlideCandidates.Add(candidate);

            Conditions.Clear();
            foreach (var condition in sourceRule.Conditions)
                Conditions.Add(condition.Copy());
        }

        #endregion
    }
}
