using System.Diagnostics;
using System.Text.Json;
using DBDStudio.Converter.Json;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Utility.Persistence;

namespace DBDStudio.Services
{
    public sealed class PersistenceService(ApplicationSettings settings, IEnumerable<IPersistable> persistables)
    {
        private readonly ApplicationSettings _settings = settings;
        private readonly IReadOnlyList<IPersistable> _persistables = [.. persistables];

        public void Load()
        {
            var workspacePath = ResolveWorkspacePath();
            if (!File.Exists(workspacePath)) {
                return;
            }

            try {
                var jsonConfig = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Local);
                var json = File.ReadAllText(workspacePath);
                var snapshot = JsonSerializer.Deserialize<PersistenceSnapshot>(json, jsonConfig);
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
                            state = JsonSerializer.Deserialize(jsonElement.GetRawText(), persistable.PersistenceStateType, jsonConfig);
                        } else if (payload is not null) {
                            state = JsonSerializer.Deserialize(payload.ToString() ?? "{}", persistable.PersistenceStateType, jsonConfig);
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
                Items = []
            };
            foreach (var persistable in _persistables) {
                snapshot.Items[persistable.PersistenceKey] = persistable.SaveState();
            }

            var jsonConfig = JsonConfiguration.BuildJsonConfiguration(SerializationMode.Local);
            var json = JsonSerializer.Serialize(snapshot, jsonConfig);
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
