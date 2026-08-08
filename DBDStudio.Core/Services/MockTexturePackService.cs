using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using DynamicData;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.Core.Services
{
    public sealed class MockTexturePackService : ITexturePackService
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly ISettingsService _settingsService;
        private readonly HashSet<TexturePack> _configuredFolderTexturePacks = [];
        private readonly List<TexturePack> _resolvedTexturePacks = [];
        private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        public MockTexturePackService(IWorkspaceService workspaceService, ISettingsService settingsService)
        {
            _workspaceService = workspaceService;
            _settingsService = settingsService;

            _workspaceService.Current.TexturePacks.CollectionChanged += OnWorkspaceTexturePacksChanged;
            _settingsService.Settings.PropertyChanged += OnSettingsPropertyChanged;
            RebuildResolvedTexturePacks();
        }

        public event EventHandler? TexturePacksChanged;

        public IReadOnlyList<TexturePack> GetTexturePacks() => _resolvedTexturePacks;

        public void RefreshFromConfiguredFolders()
        {
            _configuredFolderTexturePacks.Clear();

            var populateFilePacks = (string folder) =>
            {
                try {
                    foreach (var pack in DiscoverExternalPacks(folder)) {
                        _configuredFolderTexturePacks.Add(pack);
                    }
                } catch (Exception ex) {
                    Debug.WriteLine($"Failed to discover texture packs in folder '{folder}': {ex.Message}");
                }
            };
            populateFilePacks(_settingsService.Settings.SkyrimDataFolder);
            populateFilePacks(_settingsService.Settings.ModsFolder);

            RebuildResolvedTexturePacks();
        }

        /// <summary>
        /// Adds a new texture pack to the workspace. If a texture pack with the same name already exists,
        /// it appends a numeric suffix to the name to ensure uniqueness.
        /// </summary>
        /// <param name="pack">The texture pack to add.</param>
        public void Add(TexturePack pack)
        {
            // Strip a trailing " (N)" suffix, if present.
            var baseName = System.Text.RegularExpressions.Regex.Replace(pack.Name, @"\s*\(\d+\)$", string.Empty);
            var regex = new System.Text.RegularExpressions.Regex(
                $@"^{System.Text.RegularExpressions.Regex.Escape(baseName)}\s\((\d+)\)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var hasBaseName = _resolvedTexturePacks.Any(existingPack =>
                existingPack.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase));

            var maxSuffix = _resolvedTexturePacks
                .Select(existingPack => regex.Match(existingPack.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .DefaultIfEmpty(hasBaseName ? 0 : -1)
                .Max();

            if (maxSuffix > -1)
                pack.Name = $"{baseName} ({maxSuffix + 1})";
            else
                pack.Name = baseName;

            _workspaceService.Current.TexturePacks.Add(pack);
        }

        public void TryAdd(TexturePack pack)
        {
            if (_workspaceService.Current.TexturePacks.Contains(pack)) {
                return;
            }
            _workspaceService.Current.TexturePacks.Add(pack);
        }

        public void Remove(TexturePack pack) => _workspaceService.Current.TexturePacks.Remove(pack);

        public TexturePackState GetTexturePackState(TexturePack pack)
        {
            var success = _configuredFolderTexturePacks.TryGetValue(pack, out var originalPack);
            if (!success || originalPack is null)
                return TexturePackState.Ephemeral;
            if (pack.LastUpdatedUtc != originalPack.LastUpdatedUtc)
                return TexturePackState.DiskEdited;
            return TexturePackState.Disk;
        }

        public void ResetToDiskState(TexturePack pack)
        {
            if (!_configuredFolderTexturePacks.Contains(pack))
                return;
            _workspaceService.Current.TexturePacks.Remove(pack);
        }

        private void OnWorkspaceTexturePacksChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildResolvedTexturePacks();

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or nameof(ApplicationSettings.ModsFolder)) {
                RefreshFromConfiguredFolders();
            }
        }

        private void RebuildResolvedTexturePacks()
        {
            var uids = new HashSet<Guid>();
            _resolvedTexturePacks.Clear();

            var appendUniquePacks = (IEnumerable<TexturePack> packs) =>
            {
                foreach (var pack in packs) {
                    var key = pack.Uid;
                    if (!uids.Add(key))
                        continue;
                    _resolvedTexturePacks.Add(pack);
                }
            };
            appendUniquePacks(_workspaceService.Current.TexturePacks);
            appendUniquePacks(_configuredFolderTexturePacks.Select(pack => pack.Clone()));

            _resolvedTexturePacks.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            TexturePacksChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Discovers and builds texture packs in the specified root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture packs.</param>
        /// <param name="origin">The origin of the texture packs.</param>
        /// <returns>An enumerable of TexturePack instances discovered in the root folder.</returns>
        /// <remarks>Exceptions are forwarded to the caller. The method does not handle exceptions internally.</remarks>
        private IEnumerable<TexturePack> DiscoverExternalPacks(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder)) {
                yield break;
            }

            foreach (var configFile in EnumerateConfigFiles(rootFolder)) {
                var pack = TryReadPackFromConfig(configFile);
                if (pack is not null) {
                    yield return pack;
                }
            }
        }

        /// <summary>
        /// Searches for texture pack configuration files in the specified root folder. It looks for config.yml files in two patterns:
        /// 1. Directly under the root folder in the path: rootFolder/textures/dbd/*/config.yml
        /// 2. In any subdirectory of the root folder in the path: rootFolder/*/textures/dbd/*/config.yml
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture pack configuration files.</param>
        /// <returns>An enumerable of FileInfo objects representing the found configuration files.</returns>
        /// <remarks>Exceptions are forwarded to the caller. The method does not handle exceptions internally.</remarks>
        private static IEnumerable<FileInfo> EnumerateConfigFiles(string rootFolder)
        {
            // Pattern 1: rootFolder/textures/dbd/*/config.yml
            var texturesDbd = Path.Combine(rootFolder, "textures", "dbd");
            if (Directory.Exists(texturesDbd)) {
                foreach (var packDir in Directory.EnumerateDirectories(texturesDbd)) {
                    var configFile = Path.Combine(packDir, "config.yml");
                    if (File.Exists(configFile)) {
                        yield return new FileInfo(configFile);
                    }
                }
            }
            // Pattern 2: rootFolder/*/textures/dbd/*/config.yml
            foreach (var subdir in Directory.EnumerateDirectories(rootFolder)) {
                var texturesDbdSub = Path.Combine(subdir, "textures", "dbd");
                if (Directory.Exists(texturesDbdSub)) {
                    foreach (var packDir in Directory.EnumerateDirectories(texturesDbdSub)) {
                        var configFile = Path.Combine(packDir, "config.yml");
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
        private TexturePack? TryReadPackFromConfig(FileInfo configFile)
        {
            try {
                var configDirectory = configFile.Directory?.FullName;
                if (configDirectory is null || string.IsNullOrWhiteSpace(configDirectory)) {
                    return null;
                }

                var yaml = File.ReadAllText(configFile.FullName);
                var config = _yamlDeserializer.Deserialize<TexturePackConfig>(yaml);
                if (config is null) {
                    return null;
                }

                var uid = config.Uid is not null && Guid.TryParse(config.Uid, out var guid) ? guid : Guid.NewGuid();
                var pack = new TexturePack(guid: uid) {
                    Name = config.Name?.Trim() ?? "???",
                    Description = config.Description?.Trim() ?? string.Empty,
                };

                foreach (var mapping in config.Mappings) {
                    if (string.IsNullOrWhiteSpace(mapping.Vanilla) || string.IsNullOrWhiteSpace(mapping.Replacement)) {
                        continue;
                    }

                    var normalize = (string path) => path.Replace('\\', '/').Trim();
                    var replacementPath = normalize(mapping.Replacement);
                    pack.Mappings.Add(new TextureMapping {
                        VanillaTexture = normalize(mapping.Vanilla),
                        ReplacementTexture = replacementPath,
                        SourcePath = Path.Combine(configDirectory, replacementPath.Replace('/', Path.DirectorySeparatorChar))
                    });
                }

                return pack;
            } catch (Exception ex) when (ex is not YamlException) {
                Debug.WriteLine($"Failed to read texture pack config '{configFile.FullName}': {ex.Message}");
            } catch (YamlException ex) {
                Debug.WriteLine($"Failed to parse yaml file at texture pack config '{configFile.FullName}': {ex.Message}");
            }

            return null;
        }

        // TODO: See if this can be simplified. Currently we have TexturePack.cs, this struct here and the Mirror in Workspace Settings
        // Look into using a single source of truth for the config file and the in-memory representation of a texture pack.
        private sealed class TexturePackConfig
        {
            public string? Uid { get; init; }
            public string? Name { get; init; }
            public string? Description { get; init; }
            public List<TexturePackMappingConfig> Mappings { get; init; } = [];
        }

        private sealed class TexturePackMappingConfig
        {
            public string? Vanilla { get; init; }
            public string? Replacement { get; init; }
        }
    }
}
