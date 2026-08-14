using System.ComponentModel;
using System.Diagnostics;
using DBDStudio.Core.Interfaces.Mutagen;
using DBDStudio.Core.Utility;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Masters;

namespace DBDStudio.Core.Models.Mutagen
{
    public sealed class FormDatabase : IFormDatabase
    {
        private readonly Lock _loadSync = new();
        private readonly Lock _pluginsSync = new();
        private CancellationTokenSource? _loadCancellation;
        private Task? _loadTask;
        private readonly ApplicationSettings _settings;
        private List<PluginData> _plugins = [];
        private IPluginData[] _pluginsSnapshot = [];

        public FormDatabase(ApplicationSettings settings)
        {
            _settings = settings;
            _settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ApplicationSettings.SkyrimDataFolder) || e.PropertyName == nameof(ApplicationSettings.ModsFolder)) {
                    LoadDatabase();
                }
            };
        }

        public event EventHandler<DatabaseChangedEventArgs>? DatabaseChanged;

        public IEnumerable<IPluginData> Plugins => Volatile.Read(ref _pluginsSnapshot);

        public void LoadDatabase()
        {
            CancellationTokenSource? oldCancellation;
            CancellationToken loadToken;

            try {
                _loadSync.Enter();
                oldCancellation = _loadCancellation;
                _loadCancellation = new CancellationTokenSource();
                loadToken = _loadCancellation.Token;
            } finally {
                _loadSync.Exit();
            }
            oldCancellation?.Cancel();
            if (_loadTask is not null) {
                _ = _loadTask.ContinueWith(
                    _ => oldCancellation?.Dispose(), TaskScheduler.Default);
            }

            List<PluginData> oldModList;
            _pluginsSync.Enter();
            try {
                oldModList = _plugins.ToList();
            } finally {
                _pluginsSync.Exit();
            }

            var oldByKey = oldModList.ToDictionary(p => p.Key);
            var newlyAdded = new List<IPluginData>();
            var rebuiltByKey = new Dictionary<ModKey, PluginData>();

            var details = new[] {
                new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1)
            };

            DirectoryIterator.EnumerateProjectFiles(details, "", "*.esp")
                .Concat(DirectoryIterator.EnumerateProjectFiles(details, "", "*.esm"))
                .Concat(DirectoryIterator.EnumerateProjectFiles(details, "", "*.esl"))
                .Select(fileInfo =>
                {
                    var key = ModKey.FromNameAndExtension(fileInfo.Name);
                    if (oldByKey.TryGetValue(key, out var oldMod) && oldMod.LastWriteTicksUtc >= fileInfo.LastWriteTimeUtc.Ticks)
                        return oldMod;
                    return new PluginData(
                        key,
                        fileInfo.Name,
                        fileInfo.FullName,
                        isEnabled: true,
                        fileInfo.LastWriteTimeUtc.Ticks
                    );
                })
                .ToList()
                .ForEach(plugin =>
                {
                    if (oldByKey.ContainsKey(plugin.Key) is false) {
                        newlyAdded.Add(plugin);
                    }
                    rebuiltByKey[plugin.Key] = plugin;
                });

            var rebuiltPlugins = rebuiltByKey.Values.ToList();
            var removedPlugins = oldModList.Where(p => rebuiltByKey.ContainsKey(p.Key) is false).Cast<IPluginData>().ToList();

            _pluginsSync.Enter();
            try {
                _plugins = rebuiltPlugins;
                Volatile.Write(ref _pluginsSnapshot, rebuiltPlugins.Cast<IPluginData>().ToArray());
            } finally {
                _pluginsSync.Exit();
            }

            _loadTask = LoadPluginsAsync(rebuiltPlugins.ToHashSet(), loadToken);

            if (removedPlugins.Count > 0) {
                DatabaseChanged?.Invoke(this, new DatabaseChangedEventArgs(DatabaseChangedEventArgs.DatabaseChangeType.PluginsRemoved, removedPlugins));
            }
            if (newlyAdded.Count > 0) {
                DatabaseChanged?.Invoke(this, new DatabaseChangedEventArgs(DatabaseChangedEventArgs.DatabaseChangeType.PluginsAdded, newlyAdded));
            }
        }

        private static async Task LoadPluginsAsync(IReadOnlySet<PluginData> plugins, CancellationToken cancellationToken)
        {
            try {
                await Parallel.ForEachAsync(
                    plugins,
                    new ParallelOptions {
                        MaxDegreeOfParallelism = Math.Clamp(
                            Environment.ProcessorCount / 2, 1, 8),
                        CancellationToken = cancellationToken
                    },
                    async (plugin, token) =>
                    {
                        await plugin.LoadMod();
                    });
            } catch (OperationCanceledException) {
                Debug.Write("Plugin loading was canceled.");
            }
        }
    }
}
