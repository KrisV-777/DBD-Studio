using System.ComponentModel;
using DBDStudio.Core.Models;
using DynamicData.Binding;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace DBDStudio.Core.Interfaces.Mutagen
{
    public enum PluginLoadState
    {
        NotLoaded,
        Loading,
        Loaded
    }

    public interface IPluginData : INotifyPropertyChanged
    {
        ModKey Key { get; }
        string PluginName { get; }
        bool IsEnabled { get; set; }
        IReadOnlyList<FormRecord> Records { get; }
        PluginLoadState LoadState { get; }
    }
}
