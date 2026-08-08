using System;
using System.Collections.Generic;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces
{
    public interface IFormDatabaseService
    {
        bool IsLoading { get; }
        bool IsReady { get; }
        string? StatusMessage { get; }
        event EventHandler? StatusChanged;

        IReadOnlyList<FormRecord> GetRecords();
        IReadOnlyList<FormRecord> Search(string? query);
        FormReference CreateReference(string plugin, string formId);
        FormRecord? Get(FormReference reference);
        FormRecord? GetByEditorId(string editorId);
        FormRecord? GetByFormId(string formId, string? plugin = null);
        void Initialize(string? dataFolder);
        void Refresh();
    }
}
