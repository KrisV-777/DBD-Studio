namespace DBDStudio.Models.Component
{
    public sealed class RaceMenuPreset : DBDComponent
    {
        private string _sex = Models.Sex.Female;

        public required new string Name { get; init; }
        public required string JsSlotFile { get; init; }
        public string Sex
        {
            get => _sex;
            set => SetProperty(ref _sex, value);
        }

        internal override DBDComponent Copy() => (RaceMenuPreset)MemberwiseClone();

        internal override void Import(DBDComponent source)
            => throw new NotSupportedException("Importing is not supported for RaceMenuPreset.");
    }
}
