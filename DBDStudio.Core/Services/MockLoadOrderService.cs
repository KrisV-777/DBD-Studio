using System;
using System.Collections.Generic;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockLoadOrderService : ILoadOrderService
{
    private readonly List<FormRecord> _records =
    [
        new() { DisplayName = "Lydia", EditorId = "Lydia", FormId = "0001A2B3", Plugin = "Skyrim.esm", RecordType = "Actor", WinningOverride = true },
        new() { DisplayName = "Aela The Huntress", EditorId = "AelaTheHuntress", FormId = "0001A2B4", Plugin = "Skyrim.esm", RecordType = "Actor" },
        new() { DisplayName = "Whiterun Guard", EditorId = "WhiterunGuard", FormId = "0001A2B5", Plugin = "Skyrim.esm", RecordType = "Actor" },
        new() { DisplayName = "Nord Race", EditorId = "NordRace", FormId = "0001A2B6", Plugin = "Skyrim.esm", RecordType = "Race" },
        new() { DisplayName = "Companions Faction", EditorId = "CompanionsFaction", FormId = "0001A2B7", Plugin = "Skyrim.esm", RecordType = "Faction", WinningOverride = true }
    ];

    public bool IsLoading => false;
    public bool IsReady => true;
    public string? StatusMessage => "Mock load order ready";
    public event EventHandler? StatusChanged;

    public IReadOnlyList<FormRecord> GetRecords() => _records;

    public IReadOnlyList<FormRecord> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _records;

        query = query.Trim();
        return _records
            .Where(r => r.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || r.EditorId.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || r.FormId.Contains(query, StringComparison.OrdinalIgnoreCase)
                        || r.Plugin.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void Initialize(string? gamePath) { RaiseStatusChanged(); }

    public void Refresh() { RaiseStatusChanged(); }

    private void RaiseStatusChanged() => StatusChanged?.Invoke(this, EventArgs.Empty);
}
