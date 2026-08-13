using System.Collections.ObjectModel;
using System.ComponentModel;
using DBDStudio.Core.Models.Textures;
using static DBDStudio.Core.Interfaces.TexturePackListChangedEventArgs;

namespace DBDStudio.Core.Interfaces
{
    /// <summary>
    /// Represents the state of a texture pack in relation to its underlying data and any modifications made to it.
    /// </summary>
    public enum TexturePackState
    {
        None = -1,
        /// <summary>A texture pack that has no representation outside of the application.</summary>
        Ephemeral,
        /// <summary>A texture pack that has been loaded from a file (unedited).</summary>
        Primordial,
        /// <summary>A primordial texture pack that has been modified.</summary>
        Modified,
    }

    /// <summary>
    /// Represents a texture pack that has been loaded and rendered in the application, providing access to its underlying data and state.
    /// </summary>
    public interface IRenderedTexturePack
    {
        internal TexturePack Underlying { get; }
        internal TexturePack? Primordial { get; }

        Guid Uid { get; }
        string Name { get; set; }
        string Description { get; set; }
        bool IsPrivate { get; set; }
        DateTimeOffset LastUpdatedUtc { get; }
        DateTimeOffset LastUpdatedLocal { get; }
        ReadOnlyCollection<TextureMapping> Mappings { get; }
        int NumMappings { get; }

        TexturePackState State { get; }

        IRenderedTexturePack Copy();
        bool Is(TexturePackState state);
        bool IsPrimordial();
    }

    /// <summary>
    /// Outlines the service that manages texture packs, providing methods to retrieve, add, remove, and refresh texture packs,
    /// as well as an event to notify subscribers of changes to the collection of texture packs.
    /// </summary>
    public interface ITexturePackService
    {
        /// <summary>
        /// Event triggered when the list of texture packs changes, providing details about the change.
        /// </summary>
        event EventHandler<TexturePackListChangedEventArgs>? TexturePackListChanged;
        /// <summary>
        /// Retrieves a read-only list of all texture packs currently managed by the service.
        /// </summary>
        /// <returns>A read-only list of all texture packs currently managed by the service.</returns>
        IReadOnlySet<IRenderedTexturePack> TexturePacks { get; }

        /// <summary>
        /// Emplaces a texture pack into the collection.
        /// </summary>
        /// <param name="pack">The texture pack to emplace. If null, a new ephemeral pack will be created.</param>
        /// <returns>The emplaced texture pack.</returns>
        /// <remarks>If the pack is already present in the collection, it will be replaced with the new instance.</remarks>
        void Emplace(IRenderedTexturePack? pack);

        /// <summary>
        /// Emplaces a new texture pack into the collection and performs an action on its underlying TexturePack.
        /// </summary>
        /// <param name="pack">The texture pack to emplace. If null, a new ephemeral pack will be created.</param>
        /// <param name="action">The action to perform on the texture pack.</param>
        /// <param name="suppressChangeEvent">If true, suppresses the change event during this action only.</param>
        /// <remarks>An Update/Added event will be triggered after the action completes, regardless of the suppressChangeEvent flag.</remarks>
        void EmplaceAction(IRenderedTexturePack? pack, Action<TexturePack> action, bool suppressChangeEvent = true);

        /// <summary>
        /// Removes a texture pack from the collection.
        /// </summary>
        /// <param name="pack">The texture pack to remove from the collection.</param>
        void Remove(IRenderedTexturePack pack);

        /// <summary>
        /// Resets a texture pack to its primordial state, discarding any edits made to it.
        /// </summary>
        /// <param name="pack">The texture pack to reset to its primordial state.</param>
        void Reset(IRenderedTexturePack pack);

        /// <summary>
        /// Compiles a .zip file with the contents of the texture pack and exports it to a specified destination path.
        /// </summary>
        /// <param name="pack">The texture pack to export.</param>
        /// <param name="zipFileLocation">The destination path where the texture pack will be exported to.</param>
        void Export(IRenderedTexturePack pack, string zipFileLocation);

        /// <summary>
        /// Refreshes the list of texture packs from the configured folders, 
        /// updating the collection to reflect any changes in the underlying data.
        /// </summary>
        void ResetTextureList(IReadOnlyList<TexturePack>? packs = null);
    }

    /// <summary>
    /// Provides data for the TexturePackListChanged event, indicating the type of change that occurred and the affected texture pack, if applicable.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="type">The type of change that occurred.</param>
    /// <param name="affectedPack">The texture pack affected by the change, if applicable.</param>
    public sealed class TexturePackListChangedEventArgs(string? propertyName, ChangeType type, IRenderedTexturePack? affectedPack)
        : PropertyChangedEventArgs(propertyName)
    {
        /// <summary>
        /// Defines the types of changes that can occur in the texture pack list, such as addition, removal, update, or reset of a texture pack.
        /// </summary>
        public enum ChangeType
        {
            Added,
            Removed,
            Updated,
            Reset
        }

        public ChangeType Type { get; } = type;
        public IRenderedTexturePack? AffectedPack { get; } = affectedPack;
    }
}
