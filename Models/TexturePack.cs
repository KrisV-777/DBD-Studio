using System.Collections.ObjectModel;
using Body_Distribution_Studio.ViewModels;

namespace Body_Distribution_Studio.Models;

public sealed class TextureMapping : ViewModelBase
{
    private string _vanillaTexture = string.Empty;
    private string _replacementTexture = string.Empty;

    public string VanillaTexture
    {
        get => _vanillaTexture;
        set => SetField(ref _vanillaTexture, value);
    }

    public string ReplacementTexture
    {
        get => _replacementTexture;
        set => SetField(ref _replacementTexture, value);
    }
}

public sealed class TexturePack : ViewModelBase
{
    private string _name = string.Empty;
    private string _description = string.Empty;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public ObservableCollection<TextureMapping> Mappings { get; } = [];
}
