using System.Collections.Specialized;
using System.ComponentModel;
using DBDStudio.Collections;
using DBDStudio.Models.Component.Textures;
using System.Text.Json.Serialization;

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

        public TexturePack()
        {
            Mappings.CollectionChanged += OnMappingsChanged;
        }

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
        [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
        public UniqueObservableCollection<TextureMapping> Mappings { get; private set; } = [];

        private void OnMappingsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null) {
                foreach (var oldItem in e.OldItems.OfType<TextureMapping>()) {
                    oldItem.PropertyChanged -= OnMappingPropertyChanged;
                }
            }

            if (e.NewItems is not null) {
                foreach (var newItem in e.NewItems.OfType<TextureMapping>()) {
                    newItem.PropertyChanged += OnMappingPropertyChanged;
                }
            }

            MarkUpdated();
        }

        private void OnMappingPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MarkUpdated();
        }

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

            BeginMutationTrackingSuspend();
            try {
                Name = other.Name;
                Description = other.Description;
                IsPrivate = other.IsPrivate;

                Mappings.Clear();
                foreach (var mapping in other.Mappings.Select(mapping => mapping.Clone())) {
                    Mappings.Add(mapping);
                }
            } finally {
                EndMutationTrackingSuspend();
            }
        }

        #endregion
    }
}
