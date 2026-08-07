using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using YamlDotNet.Core;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.Core.Services;

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
        foreach (var pack in _workspaceService.Current.TexturePacks)
            SubscribeToPack(pack);

        _settingsService.Settings.PropertyChanged += OnSettingsPropertyChanged;
        RebuildResolvedTexturePacks();
    }

    public event EventHandler? TexturePacksChanged;

    public IReadOnlyList<TexturePack> GetTexturePacks() => _resolvedTexturePacks;

    public void RefreshFromConfiguredFolders()
    {
        _configuredFolderTexturePacks.Clear();
        _configuredFolderTexturePacks.AddRange(DiscoverExternalPacks(_settingsService.Settings.ModsFolder));
        _configuredFolderTexturePacks.AddRange(DiscoverExternalPacks(_settingsService.Settings.SkyrimDataFolder));
        RebuildResolvedTexturePacks();
    }

    public void Add(TexturePack pack)
    {
        _workspaceService.Current.TexturePacks.Add(pack);
    }

    public void Update(TexturePack pack)
    {
        var existing = _workspaceService.Current.TexturePacks.FirstOrDefault(x => x.Name == pack.Name);
        if (existing is null)
            return;

        existing.Description = pack.Description;
        existing.Visibility = pack.Visibility;
        existing.LastUpdatedUtc = DateTimeOffset.UtcNow;
        existing.Mappings.Clear();
        foreach (var mapping in pack.Mappings)
            existing.Mappings.Add(mapping);

        RebuildResolvedTexturePacks();
    }

    public void Remove(TexturePack pack)
    {
        _workspaceService.Current.TexturePacks.Remove(pack);
    }

    private void OnWorkspaceTexturePacksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<TexturePack>())
                UnsubscribeFromPack(item);
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<TexturePack>())
                SubscribeToPack(item);
        }

        RebuildResolvedTexturePacks();
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or nameof(ApplicationSettings.ModsFolder))
            RefreshFromConfiguredFolders();
    }

    private void OnWorkspacePackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TexturePack.Name))
            RebuildResolvedTexturePacks();
    }

    private void SubscribeToPack(TexturePack pack) => pack.PropertyChanged += OnWorkspacePackPropertyChanged;

    private void UnsubscribeFromPack(TexturePack pack) => pack.PropertyChanged -= OnWorkspacePackPropertyChanged;

    private void RebuildResolvedTexturePacks()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _resolvedTexturePacks.Clear();

        AppendUniquePacks(_workspaceService.Current.TexturePacks, names);
        AppendUniquePacks(_configuredFolderTexturePacks, names);

        TexturePacksChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AppendUniquePacks(IEnumerable<TexturePack> packs, HashSet<string> names)
    {
        foreach (var pack in packs)
        {
            var key = pack.Name.Trim();
            if (!names.Add(key))
                continue;

            _resolvedTexturePacks.Add(pack);
        }
    }

    private IEnumerable<TexturePack> DiscoverExternalPacks(string rootFolder)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            yield break;

        IEnumerator<string>? configFiles = null;
        try
        {
            configFiles = Directory.EnumerateFiles(rootFolder, "config.yml", SearchOption.AllDirectories).GetEnumerator();
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"Failed to scan texture packs in '{rootFolder}': {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"Failed to scan texture packs in '{rootFolder}': {ex.Message}");
        }

        if (configFiles is null)
            yield break;

        using (configFiles)
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = configFiles.MoveNext();
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"Failed while scanning texture packs in '{rootFolder}': {ex.Message}");
                    yield break;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"Failed while scanning texture packs in '{rootFolder}': {ex.Message}");
                    yield break;
                }

                if (!moved)
                    yield break;

                var configPath = configFiles.Current;
                if (!TryGetPackFolder(configPath, out var packFolder))
                    continue;

                var pack = TryReadPackFromConfig(packFolder, configPath);
                if (pack is not null)
                    yield return pack;
            }
        }
    }

    private static bool TryGetPackFolder(string configPath, out string packFolder)
    {
        packFolder = string.Empty;

        var configFile = new FileInfo(configPath);
        var packDirectory = configFile.Directory;
        var dbdDirectory = packDirectory?.Parent;
        var texturesDirectory = dbdDirectory?.Parent;

        if (packDirectory is null || dbdDirectory is null || texturesDirectory is null)
            return false;

        if (!dbdDirectory.Name.Equals("dbd", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!texturesDirectory.Name.Equals("textures", StringComparison.OrdinalIgnoreCase))
            return false;

        packFolder = packDirectory.FullName;
        return true;
    }

    private TexturePack? TryReadPackFromConfig(string packFolder, string configPath)
    {
        try
        {
            var yaml = File.ReadAllText(configPath);
            var config = _yamlDeserializer.Deserialize<TexturePackConfig>(yaml);
            if (config is null)
                return null;

            var pack = new TexturePack
            {
                Name = string.IsNullOrWhiteSpace(config.Name) ? Path.GetFileName(packFolder) : config.Name.Trim(),
                Description = config.Description?.Trim() ?? string.Empty,
                RootPath = packFolder
            };

            foreach (var mapping in config.Mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Vanilla) || string.IsNullOrWhiteSpace(mapping.Replacement))
                    continue;

                var replacementPath = NormalizePath(mapping.Replacement);
                pack.Mappings.Add(new TextureMapping
                {
                    VanillaTexture = NormalizePath(mapping.Vanilla),
                    ReplacementTexture = replacementPath,
                    SourcePath = Path.Combine(packFolder, replacementPath.Replace('/', Path.DirectorySeparatorChar))
                });
            }

            return pack;
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"Failed to read texture pack config '{configPath}': {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"Failed to read texture pack config '{configPath}': {ex.Message}");
        }
        catch (YamlException ex)
        {
            Debug.WriteLine($"Failed to parse texture pack config '{configPath}': {ex.Message}");
        }

        return null;
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').Trim();

    private sealed class TexturePackConfig
    {
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
