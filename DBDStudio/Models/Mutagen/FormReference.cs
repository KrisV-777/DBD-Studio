namespace DBDStudio.Models.Mutagen
{
    public sealed record FormReference(string Plugin, uint FormId)
    {
        public override string ToString() => $"0x{FormId:X6}|{Plugin}";

        public bool MaybeValid() => !string.IsNullOrWhiteSpace(Plugin) && FormId != 0;
    }
}
