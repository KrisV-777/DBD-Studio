using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using DBDStudio.Core.Converter.Json;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DBDStudio.Core.Models.Textures;
using DBDStudio.Core.Persistence;
using DynamicData;
using Noggog;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.Core.Services
{
    public sealed class TexturePackService : ITexturePackService, IPersistable
    {
        private readonly ApplicationSettings _settings;
        private readonly HashSet<IRenderedTexturePack> _texturePacks = [];
        private bool _suppressChangeEvent = false;

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

        public string PersistenceKey => "texturePacks";
        public Type PersistenceStateType => typeof(TexturePackPersistenceState);

        public object? SaveState()
        {
            return new TexturePackPersistenceState {
                Packs = [.. _texturePacks.Select(pack => pack.Underlying.Clone())]
            };
        }

        public void RestoreState(object? state)
        {
            if (state is not TexturePackPersistenceState texturePackState) {
                return;
            }

            ResetTextureList([.. texturePackState.Packs]);
        }

        public void ResetTextureList(IReadOnlyList<TexturePack>? packs = null)
        {
            var temporaryPacks = _texturePacks.Select(tp => tp.Underlying)
                .Concat(packs ?? [])
                .DistinctBy(pack => pack.Uid)
                .ToArray();
            var wasChangeEventSuppressed = _suppressChangeEvent;
            _suppressChangeEvent = true;

            try {
                _texturePacks.Clear();

                // Load packs from the Skyrim data folder and mods folder, if they exist.
                var populateFilePacks = (string folder) =>
                {
                    try {
                        foreach (var pack in DiscoverExternalPacks(folder)) {
                            var existingPack = _texturePacks.FirstOrDefault(p => p.Uid == pack.Uid);
                            if (existingPack is not null) {
                                if (pack.Underlying.IsMoreRecentThan(existingPack.Underlying)) {
                                    UpdateList(pack, existingPack);
                                }
                            } else {
                                UpdateList(pack, null);
                            }
                            Debug.Assert(_texturePacks.Contains(pack));
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine($"Failed to discover texture packs in folder '{folder}': {ex.Message}");
                    }
                };
                populateFilePacks(_settings.SkyrimDataFolder);
                populateFilePacks(_settings.ModsFolder);

                foreach (var temporaryPack in temporaryPacks) {
                    var freshlyLoadedPack = _texturePacks.FirstOrDefault(pack => pack.Uid == temporaryPack.Uid);

                    if (freshlyLoadedPack is null) {
                        // Packs without a current disk representation are ephemeral.
                        UpdateList(new TexturePackData(temporaryPack), null);
                        continue;
                    }

                    Debug.Assert(freshlyLoadedPack.Primordial is not null);
                    var freshlyLoadedPrimordial = freshlyLoadedPack.Primordial!;
                    if (temporaryPack.IsMoreRecentThan(freshlyLoadedPrimordial)) {
                        // Pair the edited data with the newly loaded primordial so State is
                        // computed from the current disk version rather than the stale one.
                        var modifiedPack = new TexturePackData(temporaryPack, freshlyLoadedPrimordial);
                        UpdateList(modifiedPack, freshlyLoadedPack);
                    }
                }
            } finally {
                _suppressChangeEvent = wasChangeEventSuppressed;
            }

            RaiseTexturePackListChanged(TexturePackListChangedEventArgs.ChangeType.Reset, null);
        }

        IReadOnlyList<IRenderedTexturePack> ITexturePackService.GetTexturePacks() => [.. _texturePacks.Select(pack => (IRenderedTexturePack)pack)];

        /// <summary>
        /// Adds a new texture pack to the workspace. If a texture pack with the same name already exists,
        /// it appends a numeric suffix to the name to ensure uniqueness.
        /// </summary>
        /// <param name="pack">The texture pack to add.</param>
        public void Emplace(IRenderedTexturePack? pack)
        {
            pack = ValidatePackName(pack);
            UpdateList(pack, null);
        }

        public void EmplaceAction(IRenderedTexturePack? pack, Action<TexturePack> action, bool suppressChangeEvent = true)
        {
            var updatedPack = ValidatePackName(pack);
            try {
                _suppressChangeEvent = suppressChangeEvent;
                action(updatedPack.Underlying);
            } finally {
                _suppressChangeEvent = false;
            }
            UpdateList(updatedPack, pack);
        }

        public void Remove(IRenderedTexturePack pack)
        {
            Debug.Assert(!pack.IsPrimordial(), "Cannot remove a primordial pack.");
            UpdateList(null, pack);
        }

        public void Reset(IRenderedTexturePack pack)
        {
            Debug.Assert(pack.IsPrimordial(), "Cannot reset a non-primordial pack.");
            UpdateList(new TexturePackData(pack.Primordial!.Clone(), pack.Primordial!), pack);
        }

        public void Export(IRenderedTexturePack pack, string zipFileLocation)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(zipFileLocation), "Selected pack must have a valid name for export.");
            if (pack.Underlying.Mappings.Count == 0) {
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

        #region Events

        public event EventHandler<TexturePackListChangedEventArgs>? TexturePackListChanged;

        private void UpdateList(IRenderedTexturePack? add = null, IRenderedTexturePack? remove = null)
        {
            TexturePackListChangedEventArgs.ChangeType type;
            if (add is not null && remove is not null) {
                Debug.Assert(add.Uid == remove.Uid, "Cannot update packs with different UIDs.");
                if (!_texturePacks.Remove(remove)) {
                    Debug.WriteLine($"Pack with UID {remove.Uid} does not exist.");
                    return;
                }
                var success = _texturePacks.Add(add);
                Debug.Assert(success, "Excuse me what");
                type = TexturePackListChangedEventArgs.ChangeType.Updated;
            } else if (add is not null) {
                if (!_texturePacks.Add(add)) {
                    Debug.WriteLine($"Pack with UID {add.Uid} already exists.");
                    return;
                }
                type = TexturePackListChangedEventArgs.ChangeType.Added;
            } else if (remove is not null) {
                if (!_texturePacks.Remove(remove)) {
                    Debug.WriteLine($"Pack with UID {remove.Uid} does not exist.");
                    return;
                }
                type = TexturePackListChangedEventArgs.ChangeType.Removed;
            } else {
                Debug.Assert(false, "UpdateList called with both add and remove as null.");
                return;
            }

            RaiseTexturePackListChanged(type, add ?? remove);
        }

        private void RaiseTexturePackListChanged(
            TexturePackListChangedEventArgs.ChangeType type,
            IRenderedTexturePack? affectedPack)
        {
            if (_suppressChangeEvent)
                return;

            TexturePackListChanged?.Invoke(this, new TexturePackListChangedEventArgs(
                nameof(_texturePacks), type, affectedPack
            ));
        }

        #endregion

        #region Private Methods

        private IRenderedTexturePack ValidatePackName(IRenderedTexturePack? pack = null)
        {
            pack ??= new TexturePackData(new TexturePack(Guid.NewGuid(), "New Pack", string.Empty, false, DateTimeOffset.UtcNow, []));

            if (_texturePacks.Contains(pack)) {
                Debug.WriteLine($"Pack with UID {pack.Uid} already exists.");
                return pack;
            }

            // Strip a trailing " (N)" suffix, if present.
            var baseName = System.Text.RegularExpressions.Regex.Replace(pack.Name, @"\s*\(\d+\)$", string.Empty);
            var regex = new System.Text.RegularExpressions.Regex(
                $@"^{System.Text.RegularExpressions.Regex.Escape(baseName)}\s\((\d+)\)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var hasBaseName = _texturePacks.Any(existingPack =>
                existingPack.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase));

            var maxSuffix = _texturePacks
                .Select(existingPack => regex.Match(existingPack.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .DefaultIfEmpty(hasBaseName ? 0 : -1)
                .Max();

            if (maxSuffix > -1) {
                pack.Underlying.Name = $"{baseName} ({maxSuffix + 1})";
            }
            return pack;
        }

        /// <summary>
        /// Discovers and builds texture packs in the specified root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture packs.</param>
        /// <returns>An enumerable of TexturePack instances discovered in the root folder.</returns>
        /// <exception cref="Exception">Exceptions are forwarded to the caller. The method does not handle exceptions internally.</exception>
        private static IEnumerable<TexturePackData> DiscoverExternalPacks(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder)) {
                yield break;
            }

            foreach (var configFile in EnumerateConfigFiles(rootFolder)) {
                var primordial = TryReadPackFromConfig(configFile);
                if (primordial is null) {
                    Debug.WriteLine($"Failed to read texture pack from config file '{configFile.FullName}'.");
                    continue;
                }
                var renderPack = new TexturePackData(primordial.Clone(), primordial);
                yield return renderPack;
            }
        }

        /// <summary>
        /// Searches for texture pack configuration files in the specified root folder. It looks for config.yml files in two patterns:
        /// 1. Directly under the root folder in the path: rootFolder/textures/dbd/*/config.json
        /// 2. In any subdirectory of the root folder in the path: rootFolder/*/textures/dbd/*/config.json
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture pack configuration files.</param>
        /// <returns>An enumerable of FileInfo objects representing the found configuration files.</returns>
        /// <exception cref="Exception">Exceptions are forwarded to the caller. The method does not handle exceptions internally.</exception>
        private static IEnumerable<FileInfo> EnumerateConfigFiles(string rootFolder)
        {
            // Pattern 1: rootFolder/textures/dbd/*/config.json
            var texturesDbd = Path.Combine(rootFolder, "textures", "dbd");
            if (Directory.Exists(texturesDbd)) {
                foreach (var packDir in Directory.EnumerateDirectories(texturesDbd)) {
                    var configFile = Path.Combine(packDir, "config.json");
                    if (File.Exists(configFile)) {
                        yield return new FileInfo(configFile);
                    }
                }
            }
            // Pattern 2: rootFolder/*/textures/dbd/*/config.json
            foreach (var subdir in Directory.EnumerateDirectories(rootFolder)) {
                var texturesDbdSub = Path.Combine(subdir, "textures", "dbd");
                if (Directory.Exists(texturesDbdSub)) {
                    foreach (var packDir in Directory.EnumerateDirectories(texturesDbdSub)) {
                        var configFile = Path.Combine(packDir, "config.json");
                        if (File.Exists(configFile)) {
                            yield return new FileInfo(configFile);
                        }
                    }
                }
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
    }
}
