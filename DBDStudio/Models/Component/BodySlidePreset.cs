namespace DBDStudio.Models.Component
{
    public sealed class BodySlidePreset : DBDComponent
    {
        private bool _isPrivate = false;

        public new required string Name { get; init; }
        public required string SourceXml { get; init; }
        public bool IsPrivate
        {
            get => _isPrivate;
            set => SetProperty(ref _isPrivate, value);
        }

        internal override DBDComponent Copy() => (BodySlidePreset)MemberwiseClone();

        internal override void Import(DBDComponent source)
            => throw new NotSupportedException("Importing is not supported for BodySlidePreset.");
    }
}
