using System.Diagnostics;
using System.Text.Json;
using DBDStudio.Core.Converter.Json;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Utility.Persistence;

namespace DBDStudio.Core.Services
{
    public sealed class PersistenceManager(ApplicationSettings settings, IEnumerable<IPersistable> persistables)
    {
        private readonly ApplicationSettings _settings = settings;
        private readonly IReadOnlyList<IPersistable> _persistables = persistables.ToArray();

        public void Load()
        {
            var workspacePath = ResolveWorkspacePath();
            if (!File.Exists(workspacePath)) {
                return;
            }

            try {
                JsonConfiguration.Mode = SerializationMode.Local;
                var json = File.ReadAllText(workspacePath);
                var snapshot = JsonSerializer.Deserialize<PersistenceSnapshot>(json, JsonConfiguration.Configuration);
                if (snapshot is null || snapshot.SchemaVersion != PersistenceSnapshot.CurrentSchemaVersion) {
                    return;
                }

                foreach (var persistable in _persistables) {
                    if (!snapshot.Items.TryGetValue(persistable.PersistenceKey, out var payload)) {
                        continue;
                    }

                    try {
                        object? state = null;
                        if (payload is JsonElement jsonElement) {
                            state = JsonSerializer.Deserialize(jsonElement.GetRawText(), persistable.PersistenceStateType, JsonConfiguration.Configuration);
                        } else if (payload is not null) {
                            state = JsonSerializer.Deserialize(payload.ToString() ?? "{}", persistable.PersistenceStateType, JsonConfiguration.Configuration);
                        }

                        persistable.RestoreState(state);
                    } catch (Exception ex) {
                        Debug.WriteLine($"Failed to restore persistable '{persistable.PersistenceKey}': {ex.Message}");
                    }
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to load workspace from '{workspacePath}': {ex.Message}");
            }
        }

        public void Save()
        {
            var workspacePath = ResolveWorkspacePath();
            var snapshot = new PersistenceSnapshot {
                Items = new Dictionary<string, object?>()
            };
            foreach (var persistable in _persistables) {
                snapshot.Items[persistable.PersistenceKey] = persistable.SaveState();
            }

            JsonConfiguration.Mode = SerializationMode.Local;
            var json = JsonSerializer.Serialize(snapshot, JsonConfiguration.Configuration);
            var directory = Path.GetDirectoryName(workspacePath);
            if (!string.IsNullOrWhiteSpace(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(workspacePath, json);
        }

        private string ResolveWorkspacePath()
        {
            var configured = _settings.WorkspaceFilePath;
            Debug.Assert(!string.IsNullOrWhiteSpace(configured));

            return configured.EndsWith(".dbdproj", StringComparison.OrdinalIgnoreCase)
                ? configured
                : configured + ".dbdproj";
        }
    }
}
