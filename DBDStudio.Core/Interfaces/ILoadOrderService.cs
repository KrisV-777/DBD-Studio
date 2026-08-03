using System;
using System.Collections.Generic;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces;

public interface ILoadOrderService
{
    bool IsLoading { get; }
    bool IsReady { get; }
    string? StatusMessage { get; }
    event EventHandler? StatusChanged;

    IReadOnlyList<FormRecord> GetRecords();
    IReadOnlyList<FormRecord> Search(string? query);
    void Initialize(string? gamePath);
    void Refresh();
}
