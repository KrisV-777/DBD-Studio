using System.Collections.ObjectModel;

namespace DBDStudio.Models.Component
{
    public sealed class Candidate : ModelBase
    {
        private bool _isExclusive = false;

        public string Name { get; init; } = string.Empty;
        public bool IsExclusive {
            get => _isExclusive;
            set => SetProperty(ref _isExclusive, value);
        }
    }
}
