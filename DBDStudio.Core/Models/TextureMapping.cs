using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBDStudio.Core.Models;

public sealed class TextureMapping : INotifyPropertyChanged
{
    private string _vanillaTexture = string.Empty;
    private string _replacementTexture = string.Empty;
    private string _sourcePath = string.Empty;

    public string VanillaTexture
    {
        get => _vanillaTexture;
        set => SetProperty(ref _vanillaTexture, value);
    }

    public string ReplacementTexture
    {
        get => _replacementTexture;
        set => SetProperty(ref _replacementTexture, value);
    }

    public string SourcePath
    {
        get => _sourcePath;
        set => SetProperty(ref _sourcePath, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override bool Equals(object? obj)
    {
        if (obj is not TextureMapping other)
            return false;
        return _vanillaTexture == other._vanillaTexture;
    }

    public override int GetHashCode()
    {
        return _vanillaTexture.GetHashCode();
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
