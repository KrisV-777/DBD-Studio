using static DBDStudio.Interfaces.Mutagen.DatabaseChangedEventArgs;

namespace DBDStudio.Interfaces.Mutagen
{
    public interface IFormDatabase
    {
        /// <summary>
        /// Dispatches asynchronous events when the underlying database changes
        /// </summary>
        event EventHandler<DatabaseChangedEventArgs>? DatabaseChanged;

        /// <summary>
        /// Gets the collection of plugins currently loaded in the database.
        /// </summary>
        IEnumerable<IPluginData> Plugins { get; }

        void LoadDatabase();
    }

    public class DatabaseChangedEventArgs(DatabaseChangeType type, IEnumerable<IPluginData>? plugins = null) : EventArgs
    {
        /// <summary>
        /// Indicates the type of change that occurred in the database.
        /// </summary>
        public enum DatabaseChangeType
        {
            PluginsAdded,
            PluginsRemoved
        }

        /// <summary>
        /// Gets the type of change that occurred in the database.
        /// </summary>
        public DatabaseChangeType Type { get; } = type;

        /// <summary>
        /// Gets the collection of plugins that were added or removed from the database, if applicable.
        /// </summary>
        public IEnumerable<IPluginData>? Plugins { get; } = plugins;
    }
}
