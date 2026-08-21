namespace DBDStudio.Models.Mutagen
{
    public sealed record FormReference(string Plugin, uint FormId)
    {
        public override string ToString() => $"{Plugin}|0x{FormId:X6}";
    }
}
