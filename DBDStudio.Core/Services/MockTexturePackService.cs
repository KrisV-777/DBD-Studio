using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private readonly List<TexturePack> _resolvedTexturePacks = [];

    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public MockTexturePackService(IWorkspaceService workspaceService, ISettingsService settingsService)
    {
        _workspaceService = workspaceService;
        _settingsService = settingsService;
        _settingsService.Settings.PropertyChanged += OnSettingsPropertyChanged;
    }

    public event EventHandler? TexturePacksChanged;

    public IReadOnlyList<TexturePack> GetTexturePacks() => _resolvedTexturePacks;

    public void RefreshFromConfiguredFolders()
    {
        var candidates = new List<DiscoveredPack>();
        var sequence = 0;

        foreach (var workspacePack in _workspaceService.Current.TexturePacks)
        {
            workspacePack.Source = TexturePackSource.Workspace;
            candidates.Add(new DiscoveredPack(workspacePack.Name, workspacePack, TexturePackSource.Workspace, sequence++));
        }

        var modPacks = DiscoverExternalPacks(_settingsService.Settings.ModsFolder, TexturePackSource.ModsFolder);
        foreach (var pack in modPacks)
            candidates.Add(new DiscoveredPack(pack.Name, pack, TexturePackSource.ModsFolder, sequence++));

        var gameDataPath = Path.Join(_settingsService.Settings.SkyrimDataFolder, "textures", "dbd");
        var gameDataPacks = DiscoverExternalPacks(gameDataPath, TexturePackSource.GameDataFolder);
        foreach (var pack in gameDataPacks)
            candidates.Add(new DiscoveredPack(pack.Name, pack, TexturePackSource.GameDataFolder, sequence++));

        var winners = ResolveConflicts(candidates)
            .OrderBy(x => x.Source)
            .ThenBy(x => x.Sequence)
            .Select(x => x.Pack)
            .ToList();

        _resolvedTexturePacks.Clear();
        _resolvedTexturePacks.AddRange(winners);
        TexturePacksChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Add(TexturePack pack)
    {
        pack.Source = TexturePackSource.Workspace;
        pack.LastUpdatedUtc = DateTimeOffset.UtcNow;
        _workspaceService.Current.TexturePacks.Add(pack);
        RefreshFromConfiguredFolders();
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

        RefreshFromConfiguredFolders();
    }

    public void Remove(TexturePack pack)
    {
        _workspaceService.Current.TexturePacks.Remove(pack);
        RefreshFromConfiguredFolders();
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ApplicationSettings.SkyrimDataFolder) or nameof(ApplicationSettings.ModsFolder))
            RefreshFromConfiguredFolders();
    }

    private IEnumerable<TexturePack> DiscoverExternalPacks(string rootFolder, TexturePackSource source)
    {
        if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            yield break;

        IEnumerable<string> configFiles;
        try
        {
            configFiles = Directory.EnumerateFiles(rootFolder, "config.yml", SearchOption.AllDirectories);
        }
        catch
        {
            yield break;
        }

        foreach (var configPath in configFiles)
        {
            if (!TryGetPackFolder(configPath, out var packFolder))
                continue;

            var pack = TryReadPackFromConfig(packFolder!, configPath);
            if (pack is null)
                continue;

            pack.Source = source;
            yield return pack;
        }
    }

    private static bool TryGetPackFolder(string configPath, out string? packFolder)
    {
        packFolder = null;

        var configFile = new FileInfo(configPath);
        if (!configFile.Exists)
            return false;

        var parent = configFile.Directory;
        var dbdFolder = parent?.Parent;
        var texturesFolder = dbdFolder?.Parent;

        if (parent is null || dbdFolder is null || texturesFolder is null)
            return false;

        if (!dbdFolder.Name.Equals("dbd", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!texturesFolder.Name.Equals("textures", StringComparison.OrdinalIgnoreCase))
            return false;

        packFolder = parent.FullName;
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
                LastUpdatedUtc = ResolveTimestampUtc(config, configPath)
            };

            foreach (var mapping in config.Mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Vanilla) || string.IsNullOrWhiteSpace(mapping.Replacement))
                    continue;

                var replacement = NormalizePath(mapping.Replacement);
                var sourcePath = Path.Combine(packFolder, replacement.Replace('/', Path.DirectorySeparatorChar));

                pack.Mappings.Add(new TextureMapping
                {
                    VanillaTexture = NormalizePath(mapping.Vanilla),
                    ReplacementTexture = replacement,
                    SourcePath = sourcePath
                });
            }

            return pack;
        }
        catch
        {
            return null;
        }
    }

    private static DateTimeOffset ResolveTimestampUtc(TexturePackConfig config, string configPath)
    {
        if (!string.IsNullOrWhiteSpace(config.UpdatedUtc)
            && DateTimeOffset.TryParse(config.UpdatedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed.ToUniversalTime();
        }

        return File.GetLastWriteTimeUtc(configPath);
    }

    private static IReadOnlyList<DiscoveredPack> ResolveConflicts(IReadOnlyList<DiscoveredPack> candidates)
    {
        var winners = new Dictionary<string, DiscoveredPack>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            if (!winners.TryGetValue(candidate.Key, out var current))
            {
                winners[candidate.Key] = candidate;
                continue;
            }

            if (candidate.Pack.LastUpdatedUtc > current.Pack.LastUpdatedUtc)
            {
                winners[candidate.Key] = candidate;
                continue;
            }

            if (candidate.Pack.LastUpdatedUtc == current.Pack.LastUpdatedUtc)
            {
                // Keep the existing winner when timestamps are equal.
                continue;
            }
        }

        return winners.Values.ToList();
    }

    private static string NormalizePath(string value) => value.Replace('\\', '/').Trim();

    private sealed record DiscoveredPack(string Key, TexturePack Pack, TexturePackSource Source, int Sequence);

    private sealed class TexturePackConfig
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? UpdatedUtc { get; init; }
        public List<TexturePackMappingConfig> Mappings { get; init; } = [];
    }

    private sealed class TexturePackMappingConfig
    {
        public string? Vanilla { get; init; }
        public string? Replacement { get; init; }
    }
}
