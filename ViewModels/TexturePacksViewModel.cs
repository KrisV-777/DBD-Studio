using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.ViewModels;

public sealed class TexturePacksViewModel : ViewModelBase
{
    private readonly ITexturePackService _texturePackService;
    private TexturePack? _selectedPack;
    private TextureMapping? _selectedMapping;

    public ObservableCollection<TexturePack> Packs { get; } = [];

    public TexturePack? SelectedPack
    {
        get => _selectedPack;
        set
        {
            if (SetField(ref _selectedPack, value))
                SelectedMapping = null;
        }
    }

    public TextureMapping? SelectedMapping
    {
        get => _selectedMapping;
        set => SetField(ref _selectedMapping, value);
    }

    public ICommand AddPackCommand { get; }
    public ICommand AddPackFromFolderCommand { get; }
    public ICommand DuplicatePackCommand { get; }
    public ICommand DeletePackCommand { get; }
    public ICommand AddMappingCommand { get; }
    public ICommand DeleteMappingCommand { get; }
    public ICommand RemoveMappingCommand { get; }
    public ICommand BrowseTextureCommand { get; }
    public ICommand ExportPackCommand { get; }

    public TexturePacksViewModel(ITexturePackService texturePackService)
    {
        _texturePackService = texturePackService;
        AddPackCommand = new RelayCommand(AddPack);
        AddPackFromFolderCommand = new RelayCommand(AddPackFromFolder);
        DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
        DeletePackCommand = new RelayCommand(DeletePack, () => SelectedPack is not null);
        AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
        DeleteMappingCommand = new RelayCommand(DeleteMapping, () => SelectedPack is not null && SelectedMapping is not null);
        RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);
        BrowseTextureCommand = new RelayCommand(BrowseTexture, () => SelectedPack is not null && SelectedMapping is not null);
        ExportPackCommand = new RelayCommand(ExportPack, () => SelectedPack is not null);

        foreach (var pack in _texturePackService.GetTexturePacks())
            Packs.Add(pack);

        SelectedPack = Packs.Count > 0 ? Packs[0] : null;
    }

    private void AddPack()
    {
        var pack = new TexturePack { Name = "New Pack" };
        _texturePackService.Add(pack);
        Packs.Add(pack);
        SelectedPack = pack;
    }

    private void AddPackFromFolder()
    {
        // This will be handled with a file dialog from the UI
        // For now, we'll create a placeholder that can be triggered from UI
    }

    public void PopulatePackFromFolder(string folderPath)
    {
        if (SelectedPack is null || !Directory.Exists(folderPath))
            return;

        try
        {
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            var baseCount = SelectedPack.Mappings.Count;

            foreach (var filePath in files)
            {
                var relativePath = Path.GetRelativePath(folderPath, filePath);
                var texturePath = "textures/" + relativePath.Replace("\\", "/");
                var replacementPath = "textures/dbd/" + relativePath.Replace("\\", "/");

                var mapping = new TextureMapping
                {
                    VanillaTexture = texturePath,
                    ReplacementTexture = replacementPath,
                    SourcePath = filePath
                };
                SelectedPack.Mappings.Add(mapping);
            }

            _texturePackService.Update(SelectedPack);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error populating pack from folder: {ex.Message}");
        }
    }

    private void DuplicatePack()
    {
        if (SelectedPack is null) return;

        var copy = new TexturePack
        {
            Name = SelectedPack.Name + " (Copy)",
            Description = SelectedPack.Description
        };
        foreach (var m in SelectedPack.Mappings)
            copy.Mappings.Add(new TextureMapping { VanillaTexture = m.VanillaTexture, ReplacementTexture = m.ReplacementTexture, SourcePath = m.SourcePath });

        _texturePackService.Add(copy);
        Packs.Add(copy);
        SelectedPack = copy;
    }

    private void DeletePack()
    {
        if (SelectedPack is null) return;
        var index = Packs.IndexOf(SelectedPack);
        _texturePackService.Remove(SelectedPack);
        Packs.Remove(SelectedPack);
        SelectedPack = Packs.Count > 0 ? Packs[Math.Max(0, index - 1)] : null;
    }

    private void AddMapping()
    {
        if (SelectedPack is null) return;
        var mapping = new TextureMapping
        {
            VanillaTexture = "textures/",
            ReplacementTexture = "textures/dbd/"
        };
        SelectedPack.Mappings.Add(mapping);
        SelectedMapping = mapping;
    }

    private void DeleteMapping()
    {
        if (SelectedPack is null || SelectedMapping is null) return;
        SelectedPack.Mappings.Remove(SelectedMapping);
        SelectedMapping = null;
    }

    private void RemoveMapping()
    {
        if (SelectedPack is null || SelectedMapping is null) return;
        SelectedPack.Mappings.Remove(SelectedMapping);
        SelectedMapping = null;
    }

    private void BrowseTexture()
    {
        // This will be handled with a file dialog from the UI
        // For now, we'll create a placeholder that can be triggered from UI
    }

    public void SetSelectedMappingReplacementTexture(string filePath)
    {
        if (SelectedMapping is null || !File.Exists(filePath))
            return;

        var replacementPath = "textures/dbd/" + Path.GetFileName(filePath).Replace("\\", "/");
        SelectedMapping.ReplacementTexture = replacementPath;
        SelectedMapping.SourcePath = filePath;
    }

    private void ExportPack()
    {
        if (SelectedPack is null || SelectedPack.Mappings.Count == 0)
            return;

        try
        {
            var exportDir = GetExportDirectory();
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            var packZipPath = Path.Combine(exportDir, $"{SelectedPack.Name}.zip");
            var tempDir = Path.Combine(Path.GetTempPath(), $"TexturePack_{Guid.NewGuid()}");

            try
            {
                Directory.CreateDirectory(tempDir);

                // Create YAML config
                var config = new
                {
                    name = SelectedPack.Name,
                    description = SelectedPack.Description,
                    mappings = SelectedPack.Mappings.Select(m => new
                    {
                        vanilla = m.VanillaTexture,
                        replacement = m.ReplacementTexture
                    }).ToList()
                };

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(config);
                File.WriteAllText(Path.Combine(tempDir, "config.yml"), yaml);

                // Copy textures
                foreach (var mapping in SelectedPack.Mappings)
                {
                    if (!string.IsNullOrEmpty(mapping.SourcePath) && File.Exists(mapping.SourcePath))
                    {
                        var textureDestRelative = mapping.ReplacementTexture.Replace("textures/", "");
                        var textureDestPath = Path.Combine(tempDir, textureDestRelative.Replace("/", "\\"));
                        Directory.CreateDirectory(Path.GetDirectoryName(textureDestPath)!);
                        File.Copy(mapping.SourcePath, textureDestPath, overwrite: true);
                    }
                }

                // Create ZIP
                if (File.Exists(packZipPath))
                    File.Delete(packZipPath);
                ZipFile.CreateFromDirectory(tempDir, packZipPath);

                System.Diagnostics.Debug.WriteLine($"Pack exported to: {packZipPath}");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, recursive: true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error exporting pack: {ex.Message}");
        }
    }

    private static string GetExportDirectory()
    {
        // Try to find DBDS.exe location
        var appDir = AppContext.BaseDirectory;
        var exportDir = Path.Combine(appDir, "Export");
        return exportDir;
    }
}
