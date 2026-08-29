using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using DBDStudio.Converter.Json;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Utility;
using DBDStudio.Utility.Persistence;
using Noggog;

namespace DBDStudio.Services
{
    public sealed class TexturePackService : ITexturePackService, IPersistable
    {
        private const string TexturesDirectoryInfix = "textures/dbd/*";

        private readonly ApplicationSettings _settings;
        public ObservableCollection<TexturePackConstruct> TexturePacks { get; } = [];

        public TexturePackService(ApplicationSettings settingsService)
        {
            _settings = settingsService;
            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or nameof(ApplicationSettings.ModsFolder)) {
                    ResetTextureList();
                }
            };
        }

        public void ResetTextureList(IReadOnlyList<TexturePack>? packs = null)
        {
            var oldTexturePacks = TexturePacks
                .Select(pack => pack.Underlying)
                .Concat(packs ?? [])
                .DistinctBy(pack => pack.Uid)
                .ToArray();

            TexturePacks.Clear();

            try {
                foreach (var pack in DiscoverExternalPacks()) {
                    // Duplicate pack, e.g. one from Skyrim Data VM and the other from a mod folder
                    // Keep the more recently updated one, and discard the other.
                    var existingPack = TexturePacks.FirstOrDefault(p => p.Uid == pack.Uid);
                    if (existingPack is not null) {
                        if (pack.Underlying.IsMoreRecentThan(existingPack.Underlying)) {
                            TexturePacks.Remove(existingPack);
                            TexturePacks.Add(pack);
                        }
                    } else {
                        TexturePacks.Add(pack);
                    }
                    Debug.Assert(TexturePacks.FirstOrDefault(p => p.Uid == pack.Uid) is not null);
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to discover texture packs: {ex.Message}");
            }

            // Reconcile the old packs with the newly discovered ones (all of which are primordial)
            // If an old pack is not present in the newly discovered packs, it is ephemeral and should be added back
            // If an old pack is present, then it was primordial before. Pick the more recently updated version
            ConstructCollectionReconciler.ReconcileByUid(
                TexturePacks,
                oldTexturePacks,
                construct => construct.Uid,
                construct => construct.Underlying,
                construct => construct.Primordial,
                (component, isPrimordial) => new TexturePackConstruct(component, isPrimordial));
        }

        public TexturePackConstruct EmplaceNew(string? withName = null)
        {
            var pack = CreateNewPack(withName);
            TexturePacks.Add(pack);
            return pack;
        }

        public void Remove(TexturePackConstruct pack)
        {
            if (pack.IsPrimordialAny()) {
                throw new InvalidOperationException("Cannot remove a primordial pack.");
            }
            TexturePacks.Remove(pack);
        }

        public void Reset(TexturePackConstruct pack)
        {
            if (!pack.Is(ConstructState.Modified)) {
                throw new InvalidOperationException("Cannot reset a non-modified primordial pack.");
            }
            pack.Reset();
        }

        public void Export(TexturePackConstruct pack, string zipFileLocation)
        {
            if (string.IsNullOrWhiteSpace(zipFileLocation)) {
                throw new ArgumentException("The specified zip file location is invalid.", nameof(zipFileLocation));
            } else if (pack.Underlying.Mappings.Count == 0) {
                Debug.WriteLine("Error: Cannot export a pack with no mappings.");
                return;
            }

            try {
                if (!zipFileLocation.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    zipFileLocation += ".zip";

                var exportDir = Path.GetDirectoryName(zipFileLocation);
                if (string.IsNullOrWhiteSpace(exportDir)) {
                    Debug.WriteLine("Error: Invalid export path.");
                    return;
                }

                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                var tempDir = Path.Combine(Path.GetTempPath(), $"TexturePack_{Guid.NewGuid()}");
                var profileDir = Path.Combine(tempDir, "textures", "dbd", pack.Name);

                try {
                    Directory.CreateDirectory(profileDir);

                    // Write config.json
                    var jsonConfig = JsonConfiguration.Configuration;
                    JsonConfiguration.Mode = SerializationMode.Publish;
                    var json = JsonSerializer.Serialize(pack.Underlying, jsonConfig);
                    File.WriteAllText(Path.Combine(profileDir, "config.json"), json);

                    // Copy textures
                    foreach (var mapping in pack.Underlying.Mappings) {
                        if (!string.IsNullOrEmpty(mapping.AbsolutePath) && File.Exists(mapping.AbsolutePath)) {
                            var textureDestPath = Path.Combine(profileDir, mapping.ReplacementTexture);
                            Directory.CreateDirectory(Path.GetDirectoryName(textureDestPath)!);
                            File.Copy(mapping.AbsolutePath, textureDestPath, overwrite: true);
                        }
                    }

                    // Create ZIP
                    if (File.Exists(zipFileLocation))
                        File.Delete(zipFileLocation);
                    ZipFile.CreateFromDirectory(tempDir, zipFileLocation);

                    Debug.WriteLine($"Pack exported to: {zipFileLocation}");
                } finally {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Error exporting pack: {ex.Message}");
            }
        }

        #region Private Methods

        private TexturePackConstruct CreateNewPack(string? baseName = null)
        {
            return new TexturePackConstruct(new TexturePack {
                Name = UniqueNameGenerator.CreateUniqueName(
                    baseName,
                    "New Pack",
                    TexturePacks.Select(existingPack => existingPack.Name)),
            });
        }

        /// <summary>
        /// Discovers and builds texture packs in the specified root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture packs.</param>
        /// <returns>An enumerable of TexturePack instances discovered in the root folder.</returns>
        /// <exception cref="Exception">Exceptions are forwarded to the caller. The method does not handle exceptions internally.</exception>
        private IEnumerable<TexturePackConstruct> DiscoverExternalPacks()
        {
            foreach (var configFile in DirectoryIterator.EnumerateProjectFiles([
                    new DirectoryIterator.IteratorDetails(_settings.SkyrimDataFolder, 0),
                    new DirectoryIterator.IteratorDetails(_settings.ModsFolder, 1),
                ], TexturesDirectoryInfix, "config.json")) {
                var primordial = TryReadPackFromConfig(configFile);
                if (primordial is null) {
                    Debug.WriteLine($"Failed to read texture pack from config file '{configFile.FullName}'.");
                    continue;
                }
                yield return new TexturePackConstruct(
                    primordial, isPrimordial: true
                );
            }
        }

        /// <summary>
        /// Attempts to read a texture pack configuration from the specified config file.
        /// If successful, it returns a TexturePack instance; otherwise, it returns null.
        /// </summary>
        /// <param name="configFile">The configuration file to read the texture pack from.</param>
        /// <param name="origin">The origin of the texture pack.</param>
        /// <returns>A TexturePack instance if successful; otherwise, null.</returns>
        private static TexturePack? TryReadPackFromConfig(FileInfo configFile)
        {
            try {
                var directoryInfo = configFile.Directory;
                if (directoryInfo is null) {
                    Debug.WriteLine($"Config file '{configFile.FullName}' does not have a valid directory.");
                    return null;
                }
                if (!directoryInfo.Exists || !configFile.Exists) {
                    Debug.WriteLine($"Config file '{configFile.FullName}' is missing or does not exist.");
                    return null;
                }

                var json = File.ReadAllText(configFile.FullName);
                var jsonConfig = JsonConfiguration.Configuration;
                return JsonSerializer.Deserialize<TexturePack>(json, jsonConfig);
            } catch (Exception ex) when (ex is not JsonException) {
                Debug.WriteLine($"Failed to read texture pack config '{configFile.FullName}': {ex.Message}");
            } catch (JsonException ex) {
                Debug.WriteLine($"Failed to parse json file at texture pack config '{configFile.FullName}': {ex.Message}");
            }

            return null;
        }

        #endregion

        #region IPersistable

        public string PersistenceKey => "texturePacks";
        public Type PersistenceStateType => typeof(List<TexturePack>);

        public object? SaveState() => TexturePacks.Select(pack => pack.Underlying).ToList();

        public void RestoreState(object? state)
        {
            if (state is not List<TexturePack> texturePacks) {
                return;
            }
            ResetTextureList(texturePacks);
        }

        #endregion
    }
}
