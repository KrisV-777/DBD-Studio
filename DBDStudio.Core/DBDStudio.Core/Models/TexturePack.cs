using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBDStudio.Core.Models;

public enum TexturePackVisibility
{
    Public,
    Private
}

public sealed class TexturePack : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private TexturePackVisibility _visibility;
    private bool _randomPool;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public TexturePackVisibility Visibility
    {
        get => _visibility;
        set => SetProperty(ref _visibility, value);
    }

    public bool RandomPool
    {
        get => _randomPool;
        set => SetProperty(ref _randomPool, value);
    }

    public List<TextureMapping> Mappings { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
