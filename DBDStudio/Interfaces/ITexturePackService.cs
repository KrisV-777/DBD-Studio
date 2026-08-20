using System.Collections.ObjectModel;
using DBDStudio.Models.Component;

namespace DBDStudio.Interfaces
{
    /// <summary>
    /// Outlines the service that manages texture packs, providing methods to retrieve, add, remove, and refresh texture packs,
    /// as well as an event to notify subscribers of changes to the collection of texture packs.
    /// </summary>
    public interface ITexturePackService
    {
        /// <summary>
        /// Retrieves a read-only list of all texture packs currently managed by the service.
        /// </summary>
        /// <returns>A read-only list of all texture packs currently managed by the service.</returns>
        ObservableCollection<TexturePackConstruct> TexturePacks { get; }

        /// <summary>
        /// Emplaces a texture pack into the collection.
        /// </summary>
        /// <param name="pack">The texture pack to emplace. If null, a new ephemeral pack will be created.</param>
        /// <returns>The emplaced texture pack.</returns>
        /// <remarks>If the pack is already present in the collection, it will be replaced with the new instance.</remarks>
        void Add(TexturePackConstruct? pack);

        /// <summary>
        /// Removes a texture pack from the collection.
        /// </summary>
        /// <param name="pack">The texture pack to remove from the collection.</param>
        /// <throws cref="InvalidOperationException">Thrown if the texture pack cannot be removed from the collection.</throws>
        void Remove(TexturePackConstruct pack);

        /// <summary>
        /// Resets a texture pack to its primordial state, discarding any edits made to it.
        /// </summary>
        /// <param name="pack">The texture pack to reset to its primordial state.</param>
        /// <throws cref="InvalidOperationException">Thrown if the texture pack does not have a primordial state.</throws>
        void Reset(TexturePackConstruct pack);

        /// <summary>
        /// Compiles a .zip file with the contents of the texture pack and exports it to a specified destination path.
        /// </summary>
        /// <param name="pack">The texture pack to export.</param>
        /// <param name="zipFileLocation">The destination path where the texture pack will be exported to.</param>
        /// <throws cref="ArgumentException">Thrown if the provided zip file location is invalid.</throws>
        void Export(TexturePackConstruct pack, string zipFileLocation);
    }
}
