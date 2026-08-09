using System.Collections.ObjectModel;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models.Textures;
using Noggog;

namespace DBDStudio.ViewModels
{
    public sealed class TexturePacksViewModel : ViewModelBase
    {
        private readonly ITexturePackService _texturePackService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private IRenderedTexturePack? _selectedPack;
        private TextureMapping? _selectedMapping;

        public ObservableCollection<IRenderedTexturePack> Packs { get; } = [];

        public IRenderedTexturePack? SelectedPack
        {
            get => _selectedPack;
            set
            {
                if (ReferenceEquals(_selectedPack, value))
                    return;
                if (!SetField(ref _selectedPack, value))
                    return;
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

        public TexturePacksViewModel(ITexturePackService texturePackService, MainWindowViewModel mainWindowViewModel)
        {
            _texturePackService = texturePackService;
            _mainWindowViewModel = mainWindowViewModel;

            AddPackCommand = new RelayCommand(() => AddPack(null));
            DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
            DeletePackCommand = new RelayCommand(RemovePack, () => SelectedPack?.Is(TexturePackState.Ephemeral) ?? false);
            ResetPackCommand = new RelayCommand(ResetPack, () => SelectedPack?.Is(TexturePackState.Modified) ?? false);
            AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
            RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);

            _texturePackService.TexturePackListChanged += OnTexturePackListChanged;

            Packs.AddRange(_texturePackService.GetTexturePacks());
            SelectedPack = Packs.Count > 0 ? Packs[0] : null;
        }

        private void OnTexturePackListChanged(object? sender, TexturePackListChangedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(sender is ITexturePackService);

            switch (e.Type) {
            case TexturePackListChangedEventArgs.ChangeType.Added:
                System.Diagnostics.Debug.Assert(e.AffectedPack is not null);
                if (!Packs.Contains(e.AffectedPack)) {
                    Packs.Add(e.AffectedPack);
                    SelectedPack = e.AffectedPack;
                }
                break;
            case TexturePackListChangedEventArgs.ChangeType.Removed:
                System.Diagnostics.Debug.Assert(e.AffectedPack is not null);
                var newSelection = Packs.Count <= 1 ? null :
                        Packs[Math.Clamp(Packs.IndexOf(e.AffectedPack), 1, Packs.Count - 1) - 1];
                var wasSelected = ReferenceEquals(SelectedPack, e.AffectedPack);
                if (Packs.Remove(e.AffectedPack) && wasSelected) {
                    SelectedPack = newSelection;
                }
                break;
            case TexturePackListChangedEventArgs.ChangeType.Updated:
                System.Diagnostics.Debug.Assert(e.AffectedPack is not null);
                var index = Packs.IndexOf(e.AffectedPack);
                if (index >= 0) {
                    Packs[index] = e.AffectedPack;
                    SelectedPack = e.AffectedPack;
                }
                break;
            case TexturePackListChangedEventArgs.ChangeType.Reset:
                Packs.Clear();
                Packs.AddRange(_texturePackService.GetTexturePacks());
                SelectedPack = Packs.Count > 0 ? Packs[0] : null;
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

        private void AddPack(IRenderedTexturePack? pack = null) => _texturePackService.Emplace(pack);

        public void PopulatePackFromFolder(string folderPath, IRenderedTexturePack? pack = null)
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

            // Emplace the pack and perform the population action
            _texturePackService.EmplaceAction(pack, (it) => {
                try {
                    if (pack is null) {
                        // Newly created pack, set its name and description based on the folder
                        it.Name = rootDirectory.Name;
                        it.Description = $"Pack populated from folder:\n{rootDirectory.FullName}";
                    }
                    // Only search within the textures directory for performance (avoids scanning unrelated files in root)
                    var files = texturesDirectory.GetFiles("*.dds", SearchOption.AllDirectories);
                    var numMappings = it.Mappings.Count;
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
                        it.Mappings.Add(mapping);

                        if (numMappings != it.Mappings.Count) {
                            numMappings++;
                        } else {
                            numReplaced++;
                        }
                    }
                    DisplayMessage($"Populated pack '{it.Name}' with {numMappings - numMappingStart} new mappings and {numReplaced} replaced mappings.");
                } catch (Exception ex) {
                    DisplayMessage($"An error occurred while populating the texture pack from the folder: {ex.Message}");
                }
            });            
        }

        private void DuplicatePack()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);

            AddPack(SelectedPack.Copy());
        }

        private void RemovePack()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            System.Diagnostics.Debug.Assert(SelectedPack!.Is(TexturePackState.Ephemeral));

            _texturePackService.Remove(SelectedPack);
        }

        private void ResetPack()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            System.Diagnostics.Debug.Assert(SelectedPack?.Is(TexturePackState.Modified) ?? false);

            _texturePackService.Reset(SelectedPack);
        }

        private void AddMapping()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            _texturePackService.EmplaceAction(SelectedPack, (it) => {
                var mapping = new TextureMapping (
                    vanillaTexture: "",
                    replacementTexture: "",
                    absolutePath: ""
                );
                if (it.Mappings.Contains(mapping)) {
                    System.Diagnostics.Debug.WriteLine("Skipping default mapping addition because an identical mapping already exists.");
                    return;
                }
                it.Mappings.Add(mapping);
                SelectedMapping = mapping;
            });
        }

        private void RemoveMapping()
        {
            System.Diagnostics.Debug.Assert(SelectedPack is not null);
            System.Diagnostics.Debug.Assert(SelectedMapping is not null);

            _texturePackService.EmplaceAction(SelectedPack, (it) => {
                it.Mappings.Remove(SelectedMapping);
                SelectedMapping = null;
            });
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
            mapping.AbsolutePath = filePath;
        }

        public void ExportPack(string outputZipPath)
        {
            if (SelectedPack is null) {
                DisplayMessage("Error: No texture pack selected for export.");
                return;
            }

            try {
                _texturePackService.Export(SelectedPack, outputZipPath);
                DisplayMessage($"Successfully exported texture pack '{SelectedPack.Name}' to '{outputZipPath}'.");
            } catch (Exception ex) {
                DisplayMessage($"An error occurred while exporting the texture pack: {ex.Message}");
            }
        }

        private void DisplayMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            _mainWindowViewModel.StatusMessage = message;
        }
    }
}
