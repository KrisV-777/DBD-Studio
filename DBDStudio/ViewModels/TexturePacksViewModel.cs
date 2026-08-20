using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;
using DBDStudio.Interfaces;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Models.Component.Textures;
using Noggog;

namespace DBDStudio.ViewModels
{
    public sealed class TexturePacksViewModel : ViewModelBase
    {
        private readonly ITexturePackService _texturePackService;
        private TexturePackConstruct? _selectedPack;
        private TextureMapping? _selectedMapping;
        public ObservableCollection<TexturePackConstruct> Packs { get; } = [];

        public TexturePackConstruct? SelectedPack
        {
            get => _selectedPack;
            set
            {
                if (ReferenceEquals(_selectedPack, value))
                    return;
                _selectedPack = value;
                OnPropertyChanged();
                SelectedMapping = null;
                RefreshCommandStates();
            }
        }

        public TextureMapping? SelectedMapping
        {
            get => _selectedMapping;
            set => SetField(ref _selectedMapping, value);
        }

        public ICommand AddPackCommand { get; }
        public ICommand DuplicatePackCommand { get; }
        public ICommand DeletePackCommand { get; }
        public ICommand ResetPackCommand { get; }
        public ICommand AddMappingCommand { get; }
        public ICommand RemoveMappingCommand { get; }

        public TexturePacksViewModel(ITexturePackService texturePackService)
        {
            _texturePackService = texturePackService;

            AddPackCommand = new RelayCommand(() => AddPack(null));
            DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
            DeletePackCommand = new RelayCommand(RemovePack, () => SelectedPack?.Is(ConstructState.Ephemeral) ?? false);
            ResetPackCommand = new RelayCommand(ResetPack, () => SelectedPack?.Is(ConstructState.Modified) ?? false);
            AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
            RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);

            _texturePackService.TexturePacks.CollectionChanged += OnTexturePackListChanged;

            Packs.AddRange(_texturePackService.TexturePacks);
            SelectedPack = Packs.Count > 0 ? Packs[0] : null;
        }

        private void OnTexturePackListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(sender is ITexturePackService);

            switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null) {
                    foreach (TexturePackConstruct pack in e.NewItems) {
                        if (!Packs.Contains(pack)) {
                            Packs.Add(pack);
                            SelectedPack = pack;
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null) {
                    foreach (TexturePackConstruct pack in e.OldItems) {
                        var newSelection = Packs.Count <= 1 ? null :
                                Packs[Math.Clamp(Packs.IndexOf(pack), 1, Packs.Count - 1) - 1];
                        var wasSelected = ReferenceEquals(SelectedPack, pack);
                        if (Packs.Remove(pack) && wasSelected) {
                            SelectedPack = newSelection;
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                var currentSelection = SelectedPack?.Uid;
                Packs.Clear();
                Packs.AddRange(_texturePackService.TexturePacks);
                SelectedPack = currentSelection is not null && Packs.Any(p => p.Uid == currentSelection) ?
                    Packs.First(p => p.Uid == currentSelection) : (Packs.Count > 0 ? Packs[0] : null);
                break;
            }
            RefreshCommandStates();
        }

        private void RefreshCommandStates()
        {
            ((RelayCommand)AddPackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DuplicatePackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeletePackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ResetPackCommand).RaiseCanExecuteChanged();
            ((RelayCommand)AddMappingCommand).RaiseCanExecuteChanged();
            ((RelayCommand)RemoveMappingCommand).RaiseCanExecuteChanged();
        }

        private TexturePackConstruct AddPack(string? withName = null) => _texturePackService.EmplaceNew(withName);

        public void PopulatePackFromFolder(string folderPath, TexturePackConstruct? pack = null)
        {
            var rootDirectory = new DirectoryInfo(folderPath);
            if (!rootDirectory.Exists)
                return;

            // Validate that the textures subfolder exists - required folder structure is <folder>/textures/**
            var texturesPath = Path.Combine(folderPath, "textures");
            var texturesDirectory = new DirectoryInfo(texturesPath);
            if (!texturesDirectory.Exists) {
                Debug.WriteLine($"Error: No 'textures' folder found in the selected directory. Expected structure: <folder>/textures/");
                return;
            }

            // If no pack was provided, create a new ephemeral pack
            pack ??= AddPack(rootDirectory.Name);

            try {
                // Only search within the textures directory for performance (avoids scanning unrelated files in root)
                var files = texturesDirectory.GetFiles("*.dds", SearchOption.AllDirectories);
                var numMappings = pack.Mappings.Count;
                var numMappingStart = numMappings;
                var numReplaced = 0;

                foreach (var file in files) {
                    var relativePath = Path.GetRelativePath(texturesDirectory.FullName, file.FullName);
                    var normalizedPath = relativePath.Replace("\\", "/");

                    var mapping = new TextureMapping(
                        vanillaTexture: normalizedPath,
                        replacementTexture: normalizedPath,
                        absolutePath: file.FullName
                    );
                    pack.Mappings.Add(mapping);

                    if (numMappings != pack.Mappings.Count) {
                        numMappings++;
                    } else {
                        numReplaced++;
                    }
                }
                Debug.WriteLine($"Populated pack '{pack.Name}' with {numMappings - numMappingStart} new mappings and {numReplaced} replaced mappings.");
            } catch (Exception ex) {
                Debug.WriteLine($"An error occurred while populating the texture pack from the folder: {ex.Message}");
            }
        }

        private void DuplicatePack()
        {
            Debug.Assert(SelectedPack is not null);
            AddPack(SelectedPack.Name);
        }

        private void RemovePack()
        {
            Debug.Assert(SelectedPack is not null);
            Debug.Assert(SelectedPack!.Is(ConstructState.Ephemeral));

            _texturePackService.Remove(SelectedPack);
        }

        private void ResetPack()
        {
            Debug.Assert(SelectedPack is not null);
            Debug.Assert(SelectedPack?.Is(ConstructState.Modified) ?? false);

            _texturePackService.Reset(SelectedPack);
        }

        private void AddMapping()
        {
            Debug.Assert(SelectedPack is not null);
            var mapping = new TextureMapping(
                vanillaTexture: "",
                replacementTexture: "",
                absolutePath: ""
            );
            if (SelectedPack.Mappings.Contains(mapping)) {
                Debug.WriteLine("Skipping default mapping addition because an identical mapping already exists.");
                return;
            }
            SelectedPack.Mappings.Add(mapping);
            SelectedMapping = mapping;
        }

        private void RemoveMapping()
        {
            Debug.Assert(SelectedPack is not null);
            Debug.Assert(SelectedMapping is not null);

            SelectedPack.Mappings.Remove(SelectedMapping);
            SelectedMapping = null;
        }

        public static void SetSelectedMappingReplacementTexture(TextureMapping mapping, string filePath)
        {
            var texturesIndex = filePath.LastIndexOf(
                $"{Path.DirectorySeparatorChar}textures{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase);
            if (texturesIndex == -1) {
                Debug.WriteLine($"Error: The selected texture file '{filePath}' is not located within a 'textures' folder. Expected structure: <folder>/textures/**");
                return;
            }
            // Move index to the character after "textures/"
            texturesIndex += "textures".Length + 2;

            var relativePath = filePath[texturesIndex..].Replace("\\", "/");
            mapping.VanillaTexture = relativePath;
            mapping.ReplacementTexture = relativePath;
            mapping.AbsolutePath = filePath;
        }

        public void ExportPack(string outputZipPath)
        {
            if (SelectedPack is null) {
                Debug.WriteLine("Error: No texture pack selected for export.");
                return;
            }

            try {
                _texturePackService.Export(SelectedPack, outputZipPath);
                Debug.WriteLine($"Successfully exported texture pack '{SelectedPack.Name}' to '{outputZipPath}'.");
            } catch (Exception ex) {
                Debug.WriteLine($"An error occurred while exporting the texture pack: {ex.Message}");
            }
        }
    }
}
