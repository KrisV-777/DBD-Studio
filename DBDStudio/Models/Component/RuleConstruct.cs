using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DBDStudio.Models;

namespace DBDStudio.Models.Component
{
    [method: SetsRequiredMembers]
    public sealed class RuleConstruct(Rule underlying, bool isPrimordial = false)
        : Construct<Rule>(underlying, isPrimordial)
    {
        public string? SourceFilePath
        {
            get => Underlying.LastPublishedPath;
            set
            {
                if (string.Equals(Underlying.LastPublishedPath, value, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                Underlying.LastPublishedPath = value;
                OnPropertyChanged();
                RefreshStateCacheAndNotify();
            }
        }

        public override ConstructState State
        {
            get
            {
                var sourcePath = SourceFilePath;
                if (string.IsNullOrWhiteSpace(sourcePath) || Primordial is null) {
                    return ConstructState.Ephemeral;
                }

                string fullPath;
                try {
                    fullPath = Path.GetFullPath(sourcePath);
                } catch (Exception) {
                    return ConstructState.Ephemeral;
                }

                if (!File.Exists(fullPath)) {
                    return ConstructState.Ephemeral;
                }

                DateTimeOffset publishedLastUpdatedUtc;
                try {
                    publishedLastUpdatedUtc = File.GetLastWriteTimeUtc(fullPath);
                } catch (Exception) {
                    return ConstructState.Ephemeral;
                }

                return Underlying.LastUpdatedUtc > publishedLastUpdatedUtc
                    ? ConstructState.Modified
                    : ConstructState.Primordial;
            }
        }
    }
}
