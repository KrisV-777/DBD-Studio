using DBDStudio.Interfaces.Mutagen;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;
using FormType = DBDStudio.Interfaces.Mutagen.FormType;

namespace DBDStudio.Models.Mutagen
{
    public sealed class PluginData(
        ModKey key,
        string pluginName,
        string path,
        bool isEnabled,
        long lastWriteTicksUtc) : ModelBase, IPluginData
    {
        #region Fields
        private readonly ModKey _key = key;
        private readonly string _pluginName = pluginName;
        private readonly string _path = path;
        private readonly long _lastWriteTicksUtc = lastWriteTicksUtc;

        private bool _isEnabled = isEnabled;
        private FormRecord[] _records = [];
        private Dictionary<FormType, IEnumerable<FormRecord>> _recordsByFormKey = [];
        private int _loadStateValue = (int)PluginLoadState.NotLoaded;

        #endregion

        #region Properties

        public ISkyrimModGetter? Mod => GetMod();
        public ModKey Key => _key;
        public string PluginName => _pluginName;
        public string Path => _path;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        public IReadOnlyList<FormRecord> Records => Volatile.Read(ref _records);
        public IReadOnlyDictionary<FormType, IEnumerable<FormRecord>> RecordsByFormKey => Volatile.Read(ref _recordsByFormKey);
        public long LastWriteTicksUtc => _lastWriteTicksUtc;
        public PluginLoadState LoadState => (PluginLoadState)Volatile.Read(ref _loadStateValue);

        #endregion

        #region Data Loading

        private readonly Lock _sync = new();
        private ISkyrimModGetter? _mod;
        private Task? _loadTask;

        private void SetLoadState(PluginLoadState value)
        {
            var old = (PluginLoadState)Interlocked.Exchange(ref _loadStateValue, (int)value);
            if (old != value)
                OnPropertyChanged(nameof(LoadState));
        }

        /// <summary>
        /// Returns the fully loaded mod, or null if it has not finished loading.
        /// </summary>
        public ISkyrimModGetter? GetMod()
        {
            _sync.Enter();
            try {
                return _mod;
            } finally {
                _sync.Exit();
            }
        }

        /// <summary>
        /// Starts loading the mod in the background.
        /// If the mod is already loaded or currently loading, the existing task is returned.
        /// </summary>
        public Task LoadMod()
        {
            _sync.Enter();
            try {
                // Already loaded or currently loading
                if (LoadState != PluginLoadState.NotLoaded)
                    return _loadTask ?? Task.CompletedTask;
                Volatile.Write(ref _loadStateValue, (int)PluginLoadState.Loading);

                _loadTask = Task.Run(() =>
                {
                    try {
                        var mod = SkyrimMod.CreateFromBinaryOverlay(Path, SkyrimRelease.SkyrimSE);
                        var recordMap = ExtractRecordsFromPlugin(mod);
                        var recordsSnapshot = recordMap.Values.SelectMany(r => r).ToArray();

                        _sync.Enter();
                        try {
                            _mod = mod;
                            Volatile.Write(ref _recordsByFormKey, recordMap);
                            Volatile.Write(ref _records, recordsSnapshot);
                        } finally {
                            _sync.Exit();
                        }
                        SetLoadState(PluginLoadState.Loaded);
                    } catch {
                        SetLoadState(PluginLoadState.NotLoaded);
                        throw;
                    }
                });
                OnPropertyChanged(nameof(LoadState));
                return _loadTask;
            } finally {
                _sync.Exit();
            }
        }


        /// <summary>
        /// Extracts records from the given mod and returns them as a collection of FormRecord objects.
        /// </summary>
        /// <param name="mod">The mod from which to extract records.</param>
        /// <returns>A collection of FormRecord objects representing the extracted records.</returns>
        private Dictionary<FormType, IEnumerable<FormRecord>> ExtractRecordsFromPlugin(ISkyrimModGetter mod)
        {
            Dictionary<FormType, IEnumerable<FormRecord>> records = new() {
                [FormType.NPC] = ExtractRecordsFromGroup(mod, mod.Npcs),
                [FormType.Perk] = ExtractRecordsFromGroup(mod, mod.Perks),
                [FormType.Race] = ExtractRecordsFromGroup(mod, mod.Races),
                [FormType.FormList] = ExtractRecordsFromGroup(mod, mod.FormLists),
                [FormType.Faction] = ExtractRecordsFromGroup(mod, mod.Factions),
                [FormType.CombatStyle] = ExtractRecordsFromGroup(mod, mod.CombatStyles),
                [FormType.Keyword] = ExtractRecordsFromGroup(mod, mod.Keywords),
                [FormType.ActorRef] = ExtractRecordsFromGroup(mod, mod.EnumerateMajorRecords<IPlacedNpcGetter>())
            };

            if (Key.Name == "Skyrim") {
                records[FormType.ActorRef] = records[FormType.ActorRef].Append(
                    new FormRecord {
                        Name = "PlayerRef",
                        EditorId = "PlayerRef",
                        FormId = 0x14,
                        Plugin = "Skyrim.esm",
                        RecordType = nameof(IPlacedNpcGetter).Replace("BinaryOverlay", "")
                    }
                );
            }

            return records;
        }

        private IEnumerable<FormRecord> ExtractRecordsFromGroup(ISkyrimModGetter mod, IEnumerable<IMajorRecordGetter> records)
        {
            return records
                .Where(record => record.FormKey.ModKey == mod.ModKey)
                .Select(record => new FormRecord {
                    Name = GetRecordName(record),
                    EditorId = record.EditorID ?? "N/A",
                    FormId = record.FormKey.ID,
                    Plugin = PluginName,
                    RecordType = record.GetType().Name.Replace("BinaryOverlay", "")
                });
        }

        private static string GetRecordName(IMajorRecordGetter record)
        {
            var property = record.GetType().GetProperty("Name");

            return property?.GetValue(record) switch {
                TranslatedString name => name.String,
                string name => name,
                _ => null
            } ?? "N/A";
        }

        #endregion
    }
}
