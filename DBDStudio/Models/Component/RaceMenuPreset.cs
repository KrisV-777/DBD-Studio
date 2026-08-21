namespace DBDStudio.Models.Component
{
    public sealed class RaceMenuPreset : DBDComponent
    {
        public required new string Name { get; init; }
        public required string JslotFile { get; init; }

        internal override DBDComponent Copy() => (RaceMenuPreset)MemberwiseClone();

        internal override void Import(DBDComponent source)
            => throw new NotSupportedException("Importing is not supported for RaceMenuPreset.");
    }
}
