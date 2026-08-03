using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DBDStudio.Core.Services;

public sealed class MutagenSkyrimService : IFormDatabaseService, ILoadOrderService
{
    private static readonly HashSet<string> PluginExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".esm",
        ".esp",
        ".esl"
    };

    private readonly ISettingsService _settingsService;
    private readonly List<FormRecord> _records = [];
    private readonly Dictionary<string, FormRecord> _recordsByEditorId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FormRecord> _recordsByFormId = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();
    private Task? _indexingTask;

    public MutagenSkyrimService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        // Only re-index when the data folder path actually changes, not on every property change.
        _settingsService.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    public bool IsLoading { get; private set; }
    public bool IsReady { get; private set; }
    public string? StatusMessage { get; private set; } = "No Skyrim data folder configured.";

    public event EventHandler? StatusChanged;

    public IReadOnlyList<FormRecord> GetRecords()
    {
        lock (_syncRoot)
        {
            return _records.ToList();
        }
    }

    public IReadOnlyList<FormRecord> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetRecords();

        query = query.Trim();
        lock (_syncRoot)
        {
            return _records
                .Where(record => record.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || record.EditorId.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || record.FormId.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || record.Plugin.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || record.RecordType.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public FormReference CreateReference(string plugin, string formId) => new(plugin, formId);

    public FormRecord? Get(FormReference reference)
    {
        if (reference is null)
            return null;

        lock (_syncRoot)
        {
            return _recordsByFormId.TryGetValue(reference.FormId, out var record) ? record : null;
        }
    }

    public FormRecord? GetByEditorId(string editorId)
    {
        if (string.IsNullOrWhiteSpace(editorId))
            return null;

        lock (_syncRoot)
        {
            return _recordsByEditorId.TryGetValue(editorId, out var record) ? record : null;
        }
    }

    public FormRecord? GetByFormId(string formId, string? plugin = null)
    {
        if (string.IsNullOrWhiteSpace(formId))
            return null;

        lock (_syncRoot)
        {
            return _recordsByFormId.TryGetValue(formId, out var record) ? record : null;
        }
    }

    public void Initialize(string? dataFolder) => ScheduleRefresh(dataFolder ?? _settingsService.Settings.SkyrimDataFolder);

    public void Refresh() => ScheduleRefresh(_settingsService.Settings.SkyrimDataFolder);

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder))
            Refresh();
    }

    private void ScheduleRefresh(string? dataFolder)
    {
        // Skip if a background scan is already in progress.
        if (_indexingTask is { IsCompleted: false })
            return;

        SetState(isLoading: true, isReady: false, statusMessage: "Scanning Skyrim plugins…");
        _indexingTask = Task.Run(() => IndexFromDataFolder(dataFolder));
    }

    private void IndexFromDataFolder(string? dataFolder)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dataFolder) || !Directory.Exists(dataFolder))
            {
                SetState(isLoading: false, isReady: false, statusMessage: "Skyrim data folder is not configured or does not exist.");
                return;
            }

            var pluginFiles = Directory.EnumerateFiles(dataFolder, "*", SearchOption.TopDirectoryOnly)
                .Where(IsPluginFile)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (pluginFiles.Count == 0)
            {
                SetState(isLoading: false, isReady: false, statusMessage: "No plugin files found in the configured data folder.");
                return;
            }

            var gameRelease = InferGameRelease(dataFolder);
            var modKeys = pluginFiles
                .Select(path => new ModKey(Path.GetFileNameWithoutExtension(path)!, ModTypeForExtension(path)))
                .ToArray();

            using var environment = GameEnvironmentBuilder.Create(gameRelease)
                .WithTargetDataFolder(new DirectoryPath(dataFolder))
                .WithLoadOrder(modKeys)
                .Build();

            var records = new List<FormRecord>();
            var byEditorId = new Dictionary<string, FormRecord>(StringComparer.OrdinalIgnoreCase);
            var byFormId = new Dictionary<string, FormRecord>(StringComparer.OrdinalIgnoreCase);

            foreach (var listing in environment.LoadOrder.ListedOrder)
            {
                if (listing.Mod is not ISkyrimModGetter skyrimMod)
                    continue;

                var pluginName = listing.Mod is IModKeyed keyed ? keyed.ModKey.FileName.String : "Unknown";

                foreach (var majorRecord in skyrimMod.EnumerateMajorRecords())
                {
                    var displayName = ResolveDisplayName(majorRecord);
                    var editorId = string.IsNullOrWhiteSpace(majorRecord.EditorID) ? displayName : majorRecord.EditorID;
                    var formId = majorRecord.FormKey.IDString();
                    var recordType = ResolveRecordType(majorRecord);

                    var record = new FormRecord
                    {
                        DisplayName = displayName,
                        EditorId = editorId,
                        FormId = formId,
                        Plugin = pluginName,
                        RecordType = recordType,
                        FormKey = majorRecord.FormKey.ToString(),
                        WinningOverride = true
                    };

                    records.Add(record);
                    if (!string.IsNullOrWhiteSpace(editorId))
                        byEditorId[editorId] = record;
                    if (!string.IsNullOrWhiteSpace(formId))
                        byFormId[formId] = record;
                }
            }

            ReplaceIndex(records, byEditorId, byFormId,
                isReady: records.Count > 0,
                statusMessage: $"Indexed {records.Count:N0} records from {pluginFiles.Count} plugin(s).");
        }
        catch (Exception exception)
        {
            ReplaceIndex(
                [],
                new Dictionary<string, FormRecord>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, FormRecord>(StringComparer.OrdinalIgnoreCase),
                isReady: false,
                statusMessage: $"Indexing failed: {exception.Message}");
        }
    }

    private static bool IsPluginFile(string path)
    {
        var ext = Path.GetExtension(path);
        return !string.IsNullOrEmpty(ext) && PluginExtensions.Contains(ext);
    }

    private static ModType ModTypeForExtension(string path)
        => Path.GetExtension(path).Equals(".esl", StringComparison.OrdinalIgnoreCase) ? ModType.Light : ModType.Plugin;

    private static GameRelease InferGameRelease(string dataFolder)
        => dataFolder.Contains("VR", StringComparison.OrdinalIgnoreCase) ? GameRelease.SkyrimVR : GameRelease.SkyrimSE;

    private static string ResolveDisplayName(IMajorRecordGetter record)
    {
        if (record is ITranslatedNamedGetter translatedNamed && !string.IsNullOrWhiteSpace(translatedNamed.Name?.String))
            return translatedNamed.Name.String!;

        if (record is INamedGetter named && !string.IsNullOrWhiteSpace(named.Name))
            return named.Name!;

        return !string.IsNullOrWhiteSpace(record.EditorID) ? record.EditorID : ResolveRecordType(record);
    }

    private static string ResolveRecordType(IMajorRecordGetter record)
    {
        var typeName = record.GetType().Name;
        // Mutagen getter types are named like "NpcGetter", "RaceGetter", etc.
        const string suffix = "Getter";
        return typeName.EndsWith(suffix, StringComparison.Ordinal) ? typeName[..^suffix.Length] : typeName;
    }

    private void ReplaceIndex(
        List<FormRecord> records,
        Dictionary<string, FormRecord> byEditorId,
        Dictionary<string, FormRecord> byFormId,
        bool isReady,
        string statusMessage)
    {
        lock (_syncRoot)
        {
            _records.Clear();
            _records.AddRange(records);

            _recordsByEditorId.Clear();
            foreach (var kvp in byEditorId)
                _recordsByEditorId[kvp.Key] = kvp.Value;

            _recordsByFormId.Clear();
            foreach (var kvp in byFormId)
                _recordsByFormId[kvp.Key] = kvp.Value;
        }

        SetState(isLoading: false, isReady: isReady, statusMessage: statusMessage);
    }

    private void SetState(bool isLoading, bool isReady, string statusMessage)
    {
        lock (_syncRoot)
        {
            IsLoading = isLoading;
            IsReady = isReady;
            StatusMessage = statusMessage;
        }

        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
