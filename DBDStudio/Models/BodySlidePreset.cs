namespace DBDStudio.Models
{
    public sealed class BodySlidePreset : IEquatable<BodySlidePreset>, IComparable<BodySlidePreset>
    {
        public string Preset { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string SourceXml { get; set; } = string.Empty;
        public bool IsPrivate { get; set; } = false;

        #region Equality

        public bool Equals(BodySlidePreset? other) => other is not null && Preset == other.Preset && SourceXml == other.SourceXml;
        public override bool Equals(object? obj) => obj is BodySlidePreset other && Equals(other);
        public static bool operator ==(BodySlidePreset? left, BodySlidePreset? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(BodySlidePreset? left, BodySlidePreset? right) => !(left == right);
        public override int GetHashCode() => Preset.GetHashCode() ^ SourceXml.GetHashCode();
        public int CompareTo(BodySlidePreset? other) => other is null ? 1 : Preset.CompareTo(other.Preset, StringComparison.OrdinalIgnoreCase);
        public static bool operator <(BodySlidePreset? left, BodySlidePreset? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >(BodySlidePreset? left, BodySlidePreset? right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator <=(BodySlidePreset? left, BodySlidePreset? right) => !(left > right);
        public static bool operator >=(BodySlidePreset? left, BodySlidePreset? right) => !(left < right);

        #endregion
    }
}
