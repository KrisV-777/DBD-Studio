using System.Collections.Specialized;
using System.ComponentModel;
using DBDStudio.Collections;
using DynamicData;

namespace DBDStudio.Models.Textures
{
    /// <summary>
    /// Represents a texture pack containing texture mappings and metadata.
    /// </summary>
    /// <remarks>
    /// This class automatically tracks modifications by updating <see cref="LastUpdatedUtc"/> whenever any user-editable property changes.
    /// Computed properties (e.g., <see cref="IsPrivate"/>, <see cref="LastUpdatedLocal"/>) are automatically notified
    /// when their dependencies change, ensuring UI bindings remain in sync.
    /// </remarks>
    public sealed class TexturePack : ModelBase
    {
        #region Fields

        private readonly Guid _uid = Guid.NewGuid();
        private string _name = string.Empty;
        private string _description = string.Empty;
        private bool _isPrivate = false;
        private DateTimeOffset _lastUpdatedUtc = DateTimeOffset.MinValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier (GUID) of the texture pack.
        /// </summary>
        /// <remarks>This property is read-only and is automatically generated when the texture pack is created.</remarks>
        public Guid Uid => _uid;

        /// <summary>
        /// Gets or sets the name of the texture pack.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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
        /// The last time any property of the texture pack was updated.
        /// </summary>
        public DateTimeOffset LastUpdatedUtc => _lastUpdatedUtc;

        /// <summary>
        /// Gets the collection of texture mappings contained within this texture pack.
        /// </summary>
        public UniqueObservableCollection<TextureMapping> Mappings { get; } = [];

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the current texture pack has been edited more recently than the specified texture pack.
        /// </summary>
        public bool IsMoreRecentThan(TexturePack? other) => other is not null && LastUpdatedUtc > other.LastUpdatedUtc;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePack"/> class.
        /// </summary>
        /// <param name="guid"> An optional unique identifier (GUID) for the texture pack. If omitted, a new GUID is generated.</param>
        /// <remarks>Subscribes to the <see cref="PropertyChanged"/> event to handle automatic updates of timestamp and source tracking.</remarks>
        public TexturePack(Guid? guid = null)
        {
            _uid = guid ?? Guid.NewGuid();
            PropertyChanged += OnPropertyChanged;
            Mappings.CollectionChanged += OnPackMappingsCollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePack"/> class with the specified properties and texture mappings.
        /// </summary>
        /// <param name="guid">The unique identifier (GUID) of the texture pack.</param>
        /// <param name="name">The name of the texture pack.</param>
        /// <param name="description">The description of the texture pack.</param>
        /// <param name="isPrivate">Indicates whether the texture pack is private.</param>
        /// <param name="lastUpdatedUtc">The last updated timestamp of the texture pack.</param>
        /// <param name="textureMappings">The collection of texture mappings for the texture pack.</param>
        public TexturePack(
            Guid? guid,
            string name,
            string description,
            bool isPrivate,
            DateTimeOffset lastUpdatedUtc,
            List<TextureMapping> textureMappings) : this(guid)
        {
            _name = name ?? string.Empty;
            _description = description ?? string.Empty;
            _isPrivate = isPrivate;

            if (textureMappings is not null) {
                Mappings.AddRange(textureMappings.Select(mapping => mapping.Clone()));
            }

            _lastUpdatedUtc = lastUpdatedUtc == default ? DateTimeOffset.UtcNow : lastUpdatedUtc;
        }

        /// <summary>
        /// Copy constructor that creates a new <see cref="TexturePack"/> instance by copying the properties and mappings from an existing instance.
        /// </summary>
        /// <param name="source">The source <see cref="TexturePack"/> instance to copy from.</param>
        private TexturePack(TexturePack source, Guid? guid = null) : this(guid ?? source._uid)
        {
            _name = source._name;
            _description = source._description;
            _isPrivate = source._isPrivate;

            foreach (var mapping in source.Mappings)
                Mappings.Add(mapping.Clone());

            // Delay updating the last updated timestamp until after all properties and mappings have been copied.
            _lastUpdatedUtc = source._lastUpdatedUtc;
        }

        /// <summary>
        /// Creates a deep copy of the current <see cref="TexturePack"/> instance, including all properties and mappings.
        /// </summary>
        /// <returns>A new <see cref="TexturePack"/> instance that is a deep copy of the current instance.</returns>
        public TexturePack Clone() => new(this);

        /// <summary>
        /// Creates a copy of the current <see cref="TexturePack"/> instance with a new unique identifier (GUID).
        /// </summary>
        /// <param name="guid">An optional new unique identifier (GUID) for the copied texture pack. If omitted, a new GUID is generated.</param>
        /// <returns>A new <see cref="TexturePack"/> instance that is a copy of the current instance with a new unique identifier (GUID).</returns>
        public TexturePack Copy(Guid? guid = null) => new(this, guid ?? Guid.NewGuid());

        #endregion

        #region Event System

        /// <summary>
        /// Handles the <see cref="PropertyChanged"/> event for the underlying texture mappings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPackMappingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            e.OldItems?.OfType<TextureMapping>().ToList().ForEach(mapping => mapping.PropertyChanged -= OnPropertyChanged);
            e.NewItems?.OfType<TextureMapping>().ToList().ForEach(mapping => mapping.PropertyChanged += OnPropertyChanged);
            OnPropertyChanged(nameof(Mappings));
        }

        /// <summary>
        /// Forwards the <see cref="OnPropertyChanged"/> event to all listeners and updates last updated timestamp.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not nameof(LastUpdatedUtc)) {
                _lastUpdatedUtc = DateTimeOffset.UtcNow;
                OnPropertyChanged(nameof(LastUpdatedUtc));
            }

            if (ReferenceEquals(sender, this))
                return;

            OnPropertyChanged(e.PropertyName);

        }

        #endregion
    }
}
