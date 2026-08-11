namespace DBDStudio.Core.Models
{
    public sealed class RaceMenuPreset : IEquatable<RaceMenuPreset>, IComparable<RaceMenuPreset>
    {
        public string Name { get; set; } = string.Empty;
        public string JsSlotFile { get; set; } = string.Empty;
        public string Sex { get; set; } = "Female";

        #region Equality

        public bool Equals(RaceMenuPreset? other) => other is not null && Name == other.Name && JsSlotFile == other.JsSlotFile;
        public override bool Equals(object? obj) => obj is RaceMenuPreset other && Equals(other);
        public static bool operator ==(RaceMenuPreset? left, RaceMenuPreset? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(RaceMenuPreset? left, RaceMenuPreset? right) => !(left == right);
        public override int GetHashCode() => Name.GetHashCode() ^ JsSlotFile.GetHashCode();
        public int CompareTo(RaceMenuPreset? other) => other is null ? 1 : Name.CompareTo(other.Name, StringComparison.OrdinalIgnoreCase);
        public static bool operator <(RaceMenuPreset? left, RaceMenuPreset? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >(RaceMenuPreset? left, RaceMenuPreset? right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator <=(RaceMenuPreset? left, RaceMenuPreset? right) => !(left > right);
        public static bool operator >=(RaceMenuPreset? left, RaceMenuPreset? right) => !(left < right);

        #endregion
    }
}
