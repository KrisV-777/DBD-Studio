using System.ComponentModel;
using DBDStudio.Models.Mutagen;
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
        None,
        ActorRef,
        NPC,
        Perk,
        Race,
        FormList,
        Faction,
        CombatStyle,
        Keyword,
        Global,
        Weather,
        Location,
        Worldspace,
        Class,
        VoiceType,
        Quest,
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
