using System.Collections.Specialized;
using System.ComponentModel;
using DBDStudio.Collections;
using DBDStudio.Models.Component.Textures;

namespace DBDStudio.Models.Component
{
    /// <summary>
    /// Represents a texture pack containing texture mappings and metadata.
    /// </summary>
    /// <remarks>
    /// This class automatically tracks modifications by updating <see cref="LastUpdatedUtc"/> whenever any user-editable property changes.
    /// Computed properties (e.g., <see cref="IsPrivate"/>, <see cref="LastUpdatedLocal"/>) are automatically notified
    /// when their dependencies change, ensuring UI bindings remain in sync.
    /// </remarks>
    public sealed class TexturePack : DBDComponent
    {
        private string _description = string.Empty;
        private bool _isPrivate = false;

        #region Properties

        /// <summary>
        /// Gets or sets the description of the texture pack.
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the texture pack is public (accessible to random selection).
        /// </summary>
        public bool IsPrivate
        {
            get => _isPrivate;
            set => SetProperty(ref _isPrivate, value);
        }

        /// <summary>
        /// Gets the collection of texture mappings contained within this texture pack.
        /// </summary>
        public UniqueObservableCollection<TextureMapping> Mappings { get; private set; } = [];

        #endregion

        #region Methods

        /// <summary>
        /// Creates a deep copy of the current <see cref="TexturePack"/> instance, including all properties and mappings.
        /// </summary>
        /// <returns>A new <see cref="TexturePack"/> instance that is a deep copy of the current instance.</returns>
        internal override DBDComponent Copy()
        {
            var copy = (TexturePack)MemberwiseClone();
            copy.Mappings = [.. Mappings.Select(mapping => mapping.Clone())];
            return copy;
        }

        internal override void Import(DBDComponent source)
        {
            if (source is not TexturePack other)
                throw new ArgumentException("Source must be a TexturePack.", nameof(source));

            _name = other._name;
            _description = other._description;
            _isPrivate = other._isPrivate;

            Mappings = [.. other.Mappings.Select(mapping => mapping.Clone())];

            _lastUpdatedUtc = other._lastUpdatedUtc;
        }

        #endregion
    }
}
