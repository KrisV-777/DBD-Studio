using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DBDStudio.Core.Collections;

namespace DBDStudio.Core.Models;

public enum TexturePackVisibility
{
    Public,
    Private
}

public enum TexturePackSource
{
    Workspace,
    ModsFolder,
    GameDataFolder
}

public sealed class TexturePack : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _description = string.Empty;
    private TexturePackVisibility _visibility;
    private bool _randomPool;
    private DateTimeOffset _lastUpdatedUtc = DateTimeOffset.MinValue;
    private TexturePackSource _source = TexturePackSource.Workspace;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                TouchUpdatedUtc();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
                TouchUpdatedUtc();
        }
    }

    public TexturePackVisibility Visibility
    {
        get => _visibility;
        set
        {
            if (SetProperty(ref _visibility, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPrivate)));
                TouchUpdatedUtc();
            }
        }
    }

    public bool IsPrivate
    {
        get => Visibility == TexturePackVisibility.Private;
        set
        {
            var newVisibility = value ? TexturePackVisibility.Private : TexturePackVisibility.Public;
            if (Visibility == newVisibility)
                return;

            Visibility = newVisibility;
            if (value)
                RandomPool = false;
        }
    }

    public bool RandomPool
    {
        get => _randomPool;
        set
        {
            if (SetProperty(ref _randomPool, value))
                TouchUpdatedUtc();
        }
    }

    public DateTimeOffset LastUpdatedUtc
    {
        get => _lastUpdatedUtc;
        set => SetProperty(ref _lastUpdatedUtc, value);
    }

    public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

    public TexturePackSource Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public string SourceLabel => Source switch
    {
        TexturePackSource.Workspace => "Workspace",
        TexturePackSource.ModsFolder => "Mods",
        TexturePackSource.GameDataFolder => "Game Data",
        _ => "Unknown"
    };

    public UniqueTextureMappingCollection Mappings { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void TouchUpdatedUtc()
    {
        _lastUpdatedUtc = DateTimeOffset.UtcNow;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedUtc)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedLocal)));
    }
}
