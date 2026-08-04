using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

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
    public ICommand DuplicatePackCommand { get; }
    public ICommand DeletePackCommand { get; }
    public ICommand AddMappingCommand { get; }
    public ICommand RemoveMappingCommand { get; }
    public ICommand AutoPopulateCommand { get; }

    public TexturePacksViewModel(ITexturePackService texturePackService)
    {
        _texturePackService = texturePackService;
        AddPackCommand = new RelayCommand(AddPack);
        DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
        DeletePackCommand = new RelayCommand(DeletePack, () => SelectedPack is not null);
        AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
        RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);
        AutoPopulateCommand = new RelayCommand(() => { });

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

    private void RemoveMapping()
    {
        if (SelectedPack is null || SelectedMapping is null) return;
        SelectedPack.Mappings.Remove(SelectedMapping);
        SelectedMapping = null;
    }
}
