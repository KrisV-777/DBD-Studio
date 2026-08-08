using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.Core.Services
{
    public sealed class MockTexturePackService : ITexturePackService
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly ISettingsService _settingsService;
        private readonly List<TexturePack> _configuredFolderTexturePacks = [];
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

            var folderSkyrim = _settingsService.Settings.SkyrimDataFolder;
            try {
                _configuredFolderTexturePacks.AddRange(
                    DiscoverExternalPacks(folderSkyrim, TexturePackOrigin.GameDataFolder)
                );
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to discover texture packs in Skyrim data folder '{folderSkyrim}': {ex.Message}");
            }

            var folderMods = _settingsService.Settings.ModsFolder;
            try {
                _configuredFolderTexturePacks.AddRange(
                    DiscoverExternalPacks(folderMods, TexturePackOrigin.ModsFolder)
                );
            } catch (Exception ex) {
                Debug.WriteLine($"Failed to discover texture packs in mods folder '{folderMods}': {ex.Message}");
            }

            RebuildResolvedTexturePacks();
        }

        public void Add(TexturePack pack)
        {
            var packSnapshot = GetTexturePacks().ToList();
            var packname = pack.Name;

            // Check if packname already exists
            if (packSnapshot.Any(p => p.Name.Equals(packname, StringComparison.OrdinalIgnoreCase))) {
                // find the highest existing suffix in packname (n) format
                var regex = new System.Text.RegularExpressions.Regex(
                    $@"^{System.Text.RegularExpressions.Regex.Escape(packname)}\s*\((\d+)\)$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var maxSuffix = 0;
                foreach (var existingPack in packSnapshot) {
                    var match = regex.Match(existingPack.Name);
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var suffix)) {
                        maxSuffix = Math.Max(maxSuffix, suffix);
                    }
                }
                packname = $"{packname} ({maxSuffix + 1})";
            }

            pack.Name = packname;

            _workspaceService.Current.TexturePacks.Add(pack);
        }

        public void Remove(TexturePack pack)
        {
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

            AppendUniquePacks(_workspaceService.Current.TexturePacks, uids);
            AppendUniquePacks(_configuredFolderTexturePacks, uids);

            TexturePacksChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AppendUniquePacks(IEnumerable<TexturePack> packs, HashSet<Guid> uids)
        {
            foreach (var pack in packs) {
                var key = pack.Uid;
                if (!uids.Add(key)) {
                    continue;
                }

                _resolvedTexturePacks.Add(pack);
            }
        }

        /// <summary>
        /// Discovers and builds texture packs in the specified root folder.
        /// </summary>
        /// <param name="rootFolder">The root folder to search for texture packs.</param>
        /// <param name="origin">The origin of the texture packs.</param>
        /// <returns>An enumerable of TexturePack instances discovered in the root folder.</returns>
        /// <remarks>Exceptions are forwarded to the caller. The method does not handle exceptions internally.</remarks>
        private IEnumerable<TexturePack> DiscoverExternalPacks(string rootFolder, TexturePackOrigin origin)
        {
            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder)) {
                yield break;
            }

            foreach (var configFile in EnumerateConfigFiles(rootFolder)) {
                var pack = TryReadPackFromConfig(configFile, origin);
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
        private TexturePack? TryReadPackFromConfig(FileInfo configFile, TexturePackOrigin origin)
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
                var pack = new TexturePack(guid: uid, origin: origin) {
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
