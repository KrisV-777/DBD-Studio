using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Compression;
using System.Windows.Input;
using DBDStudio.Core.Collections;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DBDStudio.ViewModels
{
    public sealed class TexturePacksViewModel : ViewModelBase
    {
        private readonly ITexturePackService _texturePackService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private TexturePack? _selectedPack;
        private TextureMapping? _selectedMapping;

        public ObservableCollection<TexturePack> Packs { get; } = [];

        public TexturePack? SelectedPack
        {
            get => _selectedPack;
            set
            {
                if (ReferenceEquals(_selectedPack, value))
                    return;
                _selectedPack?.PropertyChanged -= OnPackPropertyChanged;
                var prev = _selectedPack;
                _selectedPack = value;
                _selectedPack?.PropertyChanged += OnPackPropertyChanged;
                if (Equals(prev, value)) {
                    // Same Uid, different instance: subscription updated, no UI change needed
                    return;
                }
                OnPropertyChanged(nameof(SelectedPack));
                SelectedMapping = null;
                RefreshCommandStates();
            }
        }

        public TextureMapping? SelectedMapping
        {
            get => _selectedMapping;
            set => SetField(ref _selectedMapping, value);
        }

        public TexturePackState SelectedPackState => SelectedPack is not null ? _texturePackService.GetTexturePackState(SelectedPack) : TexturePackState.None;


        public ICommand AddPackCommand { get; }
        public ICommand DuplicatePackCommand { get; }
        public ICommand DeletePackCommand { get; }
        public ICommand ResetPackCommand { get; }
        public ICommand AddMappingCommand { get; }
        public ICommand DeleteMappingCommand { get; }
        public ICommand RemoveMappingCommand { get; }

        public TexturePacksViewModel(ITexturePackService texturePackService, MainWindowViewModel mainWindowViewModel)
        {
            _texturePackService = texturePackService;
            _mainWindowViewModel = mainWindowViewModel;
            AddPackCommand = new RelayCommand(() => AddPack(null));
            DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
            DeletePackCommand = new RelayCommand(DeletePack, () => SelectedPackState == TexturePackState.Ephemeral);
            ResetPackCommand = new RelayCommand(ResetPack, () => SelectedPackState == TexturePackState.DiskEdited);
            AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
            DeleteMappingCommand = new RelayCommand(DeleteMapping, () => SelectedPack is not null && SelectedMapping is not null);
            RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);

            _texturePackService.TexturePacksChanged += (_, _) => ReloadFromService();

            ReloadFromService();
        }

        private void ReloadFromService()
        {
            var selectedPackUid = SelectedPack?.Uid;

            Packs.Clear();
            foreach (var pack in _texturePackService.GetTexturePacks())
                Packs.Add(pack);

            if (selectedPackUid is not null)
                SelectedPack = Packs.FirstOrDefault(pack => pack.Uid == selectedPackUid);
            SelectedPack ??= Packs.Count > 0 ? Packs[0] : null;
            RefreshCommandStates();
        }

        private void DisplayMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            _mainWindowViewModel.StatusMessage = message;
        }

        private void RefreshCommandStates()
        {
            ((RelayCommand)AddPackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DuplicatePackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeletePackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ResetPackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AddMappingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteMappingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveMappingCommand).RaiseCanExecuteChanged();

            OnPropertyChanged(nameof(SelectedPackState));
        }

        private void AddPack(TexturePack? pack = null)
        {
            pack ??= new TexturePack { Name = "New Pack" };
            _texturePackService.Add(pack);
            SelectedPack = pack;
        }

        public void PopulatePackFromFolder(string folderPath, TexturePack? pack = null)
        {
            var rootDirectory = new DirectoryInfo(folderPath);
            if (!rootDirectory.Exists)
                return;

            // Validate that the textures subfolder exists - required folder structure is <folder>/textures/**
            var texturesPath = Path.Combine(folderPath, "textures");
            var texturesDirectory = new DirectoryInfo(texturesPath);
            if (!texturesDirectory.Exists) {
                DisplayMessage($"Error: No 'textures' folder found in the selected directory. Expected structure: <folder>/textures/");
                return;
            }

            var isAddingTextures = pack is not null;
            pack ??= new TexturePack { Name = rootDirectory.Name };
            try {
                // pack.LastUpdatedUtc = DateTimeOffset.UtcNow;
                // Only search within the textures directory for performance (avoids scanning unrelated files in root)
                var files = texturesDirectory.GetFiles("*.dds", SearchOption.AllDirectories);
                var numMappings = pack.Mappings.Count;
                var numMappingStart = numMappings;
                var numReplaced = 0;

                foreach (var file in files) {
                    var relativePath = Path.GetRelativePath(texturesDirectory.FullName, file.FullName);
                    var normalizedPath = relativePath.Replace("\\", "/");

                    var mapping = new TextureMapping {
                        VanillaTexture = normalizedPath,
                        ReplacementTexture = normalizedPath,
                        SourcePath = file.FullName
                    };
                    pack.Mappings.Add(mapping);

                    if (numMappings != pack.Mappings.Count) {
                        numMappings++;
                    } else {
                        numReplaced++;
                    }
                }

                if (!isAddingTextures) {
                    AddPack(pack);
                } else {
                    var numNewMappings = numMappings - numMappingStart;
                    DisplayMessage($"Updated pack '{pack.Name}' with {numNewMappings} new mappings and {numReplaced} replaced mappings.");
                }
            } catch (Exception ex) {
                DisplayMessage($"An error occurred while populating the texture pack from the folder: {ex.Message}");
            }
        }

        private void DuplicatePack()
        {
            if (SelectedPack is null)
                return;
            AddPack(SelectedPack.Copy());
        }

        private void DeletePack()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            System.Diagnostics.Debug.Assert(SelectedPackState == TexturePackState.Ephemeral);

            var currentIndex = Packs.IndexOf(SelectedPack);
            _texturePackService.Remove(SelectedPack);
            SelectedPack = Packs.Count > 0 ? Packs[Math.Clamp(currentIndex, 0, Packs.Count - 1)] : null;
        }

        private void ResetPack()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            System.Diagnostics.Debug.Assert(SelectedPackState == TexturePackState.DiskEdited);

            var uid = SelectedPack.Uid;
            _texturePackService.ResetToDiskState(SelectedPack);
            SelectedPack = Packs.FirstOrDefault(pack => pack.Uid == uid);
        }

        private void AddMapping()
        {
            if (SelectedPack is null)
                return;
            var mapping = new TextureMapping {
                VanillaTexture = "",
                ReplacementTexture = "",
                SourcePath = ""
            };
            if (SelectedPack.Mappings.Contains(mapping)) {
                System.Diagnostics.Debug.WriteLine("Skipping default mapping addition because a mapping with the same VanillaTexture already exists.");
                return;
            }
            SelectedPack.Mappings.Add(mapping);
            SelectedMapping = mapping;
        }

        private void DeleteMapping()
        {
            if (SelectedPack is null || SelectedMapping is null)
                return;
            SelectedPack.Mappings.Remove(SelectedMapping);
            SelectedMapping = null;
        }

        private void RemoveMapping()
        {
            if (SelectedPack is null || SelectedMapping is null)
                return;
            SelectedPack.Mappings.Remove(SelectedMapping);
            SelectedMapping = null;
        }

        public void SetSelectedMappingReplacementTexture(TextureMapping mapping, string filePath)
        {
            var texturesIndex = filePath.LastIndexOf(
                $"{Path.DirectorySeparatorChar}textures{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase);
            if (texturesIndex == -1) {
                DisplayMessage($"Error: The selected texture file '{filePath}' is not located within a 'textures' folder. Expected structure: <folder>/textures/**");
                return;
            }
            // Move index to the character after "textures/"
            texturesIndex += "textures".Length + 2;

            var relativePath = filePath[texturesIndex..].Replace("\\", "/");
            mapping.VanillaTexture = relativePath;
            mapping.ReplacementTexture = relativePath;
            mapping.SourcePath = filePath;
        }

        private void OnPackPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TexturePack pack || string.IsNullOrWhiteSpace(e.PropertyName))
                return;
            _texturePackService.TryAdd(pack);
            RefreshCommandStates();
        }

        public void ExportPack(string outputZipPath)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(outputZipPath), "Selected pack must have a valid name for export.");
            if (SelectedPack is null || SelectedPack.Mappings.Count == 0)
                return;

            try {
                if (!outputZipPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    outputZipPath += ".zip";

                var exportDir = Path.GetDirectoryName(outputZipPath);
                if (string.IsNullOrWhiteSpace(exportDir)) {
                    DisplayMessage("Error: Invalid export path.");
                    return;
                }

                if (!Directory.Exists(exportDir))
                    Directory.CreateDirectory(exportDir);

                var tempDir = Path.Combine(Path.GetTempPath(), $"TexturePack_{Guid.NewGuid()}");
                var profileDir = Path.Combine(tempDir, "textures", "dbd", SelectedPack.Name);

                try {
                    Directory.CreateDirectory(profileDir);

                    // Create YAML config
                    var config = new {
                        name = SelectedPack.Name,
                        description = SelectedPack.Description,
                        updatedUtc = SelectedPack.LastUpdatedUtc,
                        mappings = SelectedPack.Mappings.Select(m => new {
                            vanilla = m.VanillaTexture,
                            replacement = m.ReplacementTexture
                        }).ToList()
                    };

                    var serializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    var yaml = serializer.Serialize(config);
                    File.WriteAllText(Path.Combine(profileDir, "config.yml"), yaml);

                    // Copy textures
                    foreach (var mapping in SelectedPack.Mappings) {
                        if (!string.IsNullOrEmpty(mapping.SourcePath) && File.Exists(mapping.SourcePath)) {
                            var textureDestPath = Path.Combine(profileDir, mapping.ReplacementTexture);
                            Directory.CreateDirectory(Path.GetDirectoryName(textureDestPath)!);
                            File.Copy(mapping.SourcePath, textureDestPath, overwrite: true);
                        }
                    }

                    // Create ZIP
                    if (File.Exists(outputZipPath))
                        File.Delete(outputZipPath);
                    ZipFile.CreateFromDirectory(tempDir, outputZipPath);

                    DisplayMessage($"Pack exported to: {outputZipPath}");
                } finally {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
            } catch (Exception ex) {
                DisplayMessage($"Error exporting pack: {ex.Message}");
            }
        }
    }
}
