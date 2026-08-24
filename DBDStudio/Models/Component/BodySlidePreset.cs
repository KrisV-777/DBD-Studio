namespace DBDStudio.Models.Component
{
    public sealed class BodySlidePreset : DBDComponent
    {
        public new required string Name { get; init; }
        public required string SourceXml { get; init; }

        internal override DBDComponent Copy() => (BodySlidePreset)MemberwiseClone();

        internal override void Import(DBDComponent source)
            => throw new NotSupportedException("Importing is not supported for BodySlidePreset.");
    }
}
