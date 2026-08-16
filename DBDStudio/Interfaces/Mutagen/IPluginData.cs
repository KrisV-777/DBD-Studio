using System.ComponentModel;
using DBDStudio.Models;
using Mutagen.Bethesda.Plugins;

namespace DBDStudio.Interfaces.Mutagen
{
    public enum PluginLoadState
    {
        NotLoaded,
        Loading,
        Loaded
    }

    public enum FormType
    {
        ActorRef,
        NPC,
        Perk,
        Race,
        FormList,
        Faction,
        CombatStyle,
        Keyword
    }

    public interface IPluginData : INotifyPropertyChanged
    {
        ModKey Key { get; }
        string PluginName { get; }
        bool IsEnabled { get; set; }
        IReadOnlyDictionary<FormType, IEnumerable<FormRecord>> RecordsByFormKey { get; }
        IReadOnlyList<FormRecord> Records { get; }
        PluginLoadState LoadState { get; }
    }
}
