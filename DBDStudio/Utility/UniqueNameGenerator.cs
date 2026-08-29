using System.Text.RegularExpressions;

namespace DBDStudio.Utility
{
    public static class UniqueNameGenerator
    {
        public static string CreateUniqueName(string? requestedBaseName, string defaultBaseName, IEnumerable<string> existingNames)
        {
            var baseName = requestedBaseName is not null
                ? Regex.Replace(requestedBaseName, @"\s*\(\d+\)$", string.Empty)
                : defaultBaseName;

            var regex = new Regex(
                $@"^{Regex.Escape(baseName)}\s\((\d+)\)$",
                RegexOptions.IgnoreCase);

            var hasBaseName = existingNames.Any(existingName =>
                existingName.Equals(baseName, StringComparison.OrdinalIgnoreCase));

            var maxSuffix = existingNames
                .Select(existingName => regex.Match(existingName))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .DefaultIfEmpty(hasBaseName ? 0 : -1)
                .Max();

            return maxSuffix > -1 ? $"{baseName} ({maxSuffix + 1})" : baseName;
        }
    }
}
