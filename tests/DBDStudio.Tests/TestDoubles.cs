using System.Collections.ObjectModel;
using DBDStudio.Interfaces;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Models.Component;

namespace DBDStudio.Tests;

internal sealed class FakeTexturePackService : ITexturePackService
{
    public ObservableCollection<TexturePackConstruct> TexturePacks { get; } = [];

    public TexturePackConstruct EmplaceNew(string? withName = null)
    {
        var pack = new TexturePackConstruct(new TexturePack { Name = withName ?? "New Pack" });
        TexturePacks.Add(pack);
        return pack;
    }

    public void Remove(TexturePackConstruct pack) => TexturePacks.Remove(pack);

    public void Reset(TexturePackConstruct pack) => pack.Reset();

    public void Export(TexturePackConstruct pack, string zipFileLocation)
    {
    }
}

internal sealed class FakeBodySlideService : IBodySlideService
{
    public ObservableCollection<BodySlidePreset> Presets { get; } = [];
}

internal sealed class FakeRaceMenuPresetService : IRaceMenuPresetService
{
    public ObservableCollection<RaceMenuPreset> Presets { get; } = [];
}

internal sealed class FakeFormDatabase : IFormDatabase
{
    public event EventHandler<DatabaseChangedEventArgs>? DatabaseChanged;

    public IEnumerable<IPluginData> Plugins => [];

    public void LoadDatabase()
    {
    }

    public void Raise(DatabaseChangedEventArgs.DatabaseChangeType type)
        => DatabaseChanged?.Invoke(this, new DatabaseChangedEventArgs(type));
}
