using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Services;

public sealed class MockSettingsService : ISettingsService
{
    public ApplicationSettings Settings { get; } = new();

    public void Load()
    {
    }

    public void Save()
    {
    }
}
