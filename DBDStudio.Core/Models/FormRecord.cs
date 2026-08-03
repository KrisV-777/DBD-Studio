namespace DBDStudio.Core.Models;

public sealed class FormRecord
{
    public string DisplayName { get; init; } = string.Empty;
    public string EditorId { get; init; } = string.Empty;
    public string FormId { get; init; } = string.Empty;
    public string Plugin { get; init; } = string.Empty;
    public string RecordType { get; init; } = string.Empty;
    public string FormKey { get; init; } = string.Empty;
    public bool WinningOverride { get; init; }
    public FormReference FormReference => new(Plugin, FormId);
}
