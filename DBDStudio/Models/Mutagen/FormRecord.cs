namespace DBDStudio.Models.Mutagen
{
    public sealed class FormRecord : IEquatable<FormRecord>, IComparable<FormRecord>
    {
        public string Name { get; init; } = string.Empty;
        public string EditorId { get; init; } = string.Empty;
        public uint FormId { get; init; } = 0;
        public string Plugin { get; init; } = string.Empty;
        public string RecordType { get; init; } = string.Empty;
        public FormReference FormReference => new(Plugin, FormId);

        #region Equality

        public bool MatchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;

            query = query.Trim();

            if (query.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                uint.TryParse(query[2..], System.Globalization.NumberStyles.HexNumber, null, out var formId)) {
                return FormId == formId;
            }

            return Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                EditorId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                FormReference.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                FormId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(FormRecord? other) => other is not null && FormId == other.FormId;
        public override bool Equals(object? obj) => obj is FormRecord other && Equals(other);
        public static bool operator ==(FormRecord? left, FormRecord? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(FormRecord? left, FormRecord? right) => !(left == right);
        public override int GetHashCode() => FormId.GetHashCode();
        public int CompareTo(FormRecord? other) => other is null ? 1 : FormId.CompareTo(other.FormId);
        public static bool operator <(FormRecord? left, FormRecord? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >(FormRecord? left, FormRecord? right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator <=(FormRecord? left, FormRecord? right) => !(left > right);
        public static bool operator >=(FormRecord? left, FormRecord? right) => !(left < right);

        #endregion
    }
}
