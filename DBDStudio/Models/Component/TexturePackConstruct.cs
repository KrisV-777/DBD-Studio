
using System.Diagnostics.CodeAnalysis;
using DBDStudio.Collections;
using DBDStudio.Models.Component.Textures;

namespace DBDStudio.Models.Component
{
    [method: SetsRequiredMembers]
    public sealed class TexturePackConstruct(TexturePack underlying, bool isPrimordial = false)
        : Construct<TexturePack>(underlying, isPrimordial)
    {
        /// <summary>
        /// Gets the description of the texture pack.
        /// </summary>
        public string Description
        {
            get => Underlying.Description;
            set => Underlying.Description = value;
        }

        /// <summary>
        /// Gets a value indicating whether the texture pack is public (accessible to random selection).
        /// </summary>
        public bool IsPrivate
        {
            get => Underlying.IsPrivate;
            set => Underlying.IsPrivate = value;
        }

        /// <summary>
        /// Gets the collection of texture mappings contained within this texture pack.
        /// </summary>
        public UniqueObservableCollection<TextureMapping> Mappings => Underlying.Mappings;
    }
}
