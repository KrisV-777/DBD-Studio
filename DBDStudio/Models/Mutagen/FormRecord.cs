namespace DBDStudio.Models
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
