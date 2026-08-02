using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Body_Distribution_Studio.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class TexturePacksViewModel : ViewModelBase
{
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

    public TexturePacksViewModel()
    {
        AddPackCommand = new RelayCommand(AddPack);
        DuplicatePackCommand = new RelayCommand(DuplicatePack, () => SelectedPack is not null);
        DeletePackCommand = new RelayCommand(DeletePack, () => SelectedPack is not null);
        AddMappingCommand = new RelayCommand(AddMapping, () => SelectedPack is not null);
        RemoveMappingCommand = new RelayCommand(RemoveMapping, () => SelectedPack is not null && SelectedMapping is not null);
        AutoPopulateCommand = new RelayCommand(() => { }); // Placeholder

        var fairSkin = new TexturePack
        {
            Name = "Fair Skin",
            Description = "High-quality fair skin texture pack for female characters."
        };
        fairSkin.Mappings.Add(new TextureMapping
        {
            VanillaTexture = "textures/actors/character/femalebody_1.dds",
            ReplacementTexture = "textures/dbd/FairSkin/femalebody_1.dds"
        });
        fairSkin.Mappings.Add(new TextureMapping
        {
            VanillaTexture = "textures/actors/character/femalebody_msn.dds",
            ReplacementTexture = "textures/dbd/FairSkin/femalebody_msn.dds"
        });

        var tempered = new TexturePack
        {
            Name = "Tempered",
            Description = "Tempered skin textures with enhanced detail."
        };
        tempered.Mappings.Add(new TextureMapping
        {
            VanillaTexture = "textures/actors/character/femalebody_1.dds",
            ReplacementTexture = "textures/dbd/Tempered/femalebody_1.dds"
        });

        var custom = new TexturePack { Name = "Custom", Description = "User-defined custom texture pack." };

        Packs.Add(fairSkin);
        Packs.Add(tempered);
        Packs.Add(custom);
        SelectedPack = fairSkin;
    }

    private void AddPack()
    {
        var pack = new TexturePack { Name = "New Pack" };
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
            copy.Mappings.Add(new TextureMapping
            {
                VanillaTexture = m.VanillaTexture,
                ReplacementTexture = m.ReplacementTexture
            });

        Packs.Add(copy);
        SelectedPack = copy;
    }

    private void DeletePack()
    {
        if (SelectedPack is null) return;
        var index = Packs.IndexOf(SelectedPack);
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
