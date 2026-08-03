namespace DBDStudio.Core.Models;

public sealed record FormReference(string Plugin, string FormId)
{
    public override string ToString() => $"{Plugin}::{FormId}";
}
