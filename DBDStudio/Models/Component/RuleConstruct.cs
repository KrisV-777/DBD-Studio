using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DBDStudio.Models;

namespace DBDStudio.Models.Component
{
    [method: SetsRequiredMembers]
    public sealed class RuleConstruct(Rule underlying, bool isPrimordial = false)
        : Construct<Rule>(underlying, isPrimordial)
    {
        private string? _sourceFilePath;

        public string? SourceFilePath
        {
            get => _sourceFilePath;
            set
            {
                if (!SetProperty(ref _sourceFilePath, value)) {
                    return;
                }

                RefreshStateCacheAndNotify();
            }
        }

        public override ConstructState State
        {
            get
            {
                var hasValidSourceFile = !string.IsNullOrWhiteSpace(SourceFilePath)
                    && File.Exists(SourceFilePath);
                if (!hasValidSourceFile || Primordial is null) {
                    return ConstructState.Ephemeral;
                }

                return Underlying.IsMoreRecentThan(Primordial)
                    ? ConstructState.Modified
                    : ConstructState.Primordial;
            }
        }
    }
}
