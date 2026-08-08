using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DBDStudio.Core.Collections;

namespace DBDStudio.Core.Models
{
    /// <summary>
    /// Defines the visibility level of a texture pack, controlling whether it's available for random selection.
    /// </summary>
    public enum TexturePackVisibility
    {
        /// <summary>The texture pack is visible and available for random selection.</summary>
        Public,
        /// <summary>The texture pack is hidden and excluded from random selection.</summary>
        Private
    } // TODO: remove ^^^^^^^^^

    /// <summary>
    /// Represents a texture pack containing texture mappings and metadata.
    /// </summary>
    /// <remarks>
    /// This class automatically tracks modifications by updating <see cref="LastUpdatedUtc"/> whenever any user-editable property changes.
    /// Computed properties (e.g., <see cref="IsPrivate"/>, <see cref="LastUpdatedLocal"/>) are automatically notified
    /// when their dependencies change, ensuring UI bindings remain in sync.
    /// </remarks>
    public sealed class TexturePack : INotifyPropertyChanged, IEquatable<TexturePack>
    {
        #region Fields
        private readonly Guid _uid = Guid.NewGuid();

        private string _name = string.Empty;
        private string _description = string.Empty;
        private TexturePackVisibility _visibility; // TODO: Remove this, just replace with a single boolean
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
        /// Gets or sets the visibility level of the texture pack.
        /// </summary>
        /// <remarks>Changing this value also updates <see cref="IsPrivate"/> and <see cref="AllowRandomSelection"/>.</remarks>
        public TexturePackVisibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the texture pack is private.
        /// </summary>
        /// <remarks>This is a computed property that reflects <see cref="Visibility"/>. Setting this property updates <see cref="Visibility"/> accordingly.</remarks>
        public bool IsPrivate
        {
            get => Visibility == TexturePackVisibility.Private;
            set
            {
                var newVisibility = value ? TexturePackVisibility.Private : TexturePackVisibility.Public;
                if (Visibility != newVisibility) {
                    Visibility = newVisibility;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the texture pack can be randomly selected.
        /// </summary>
        /// <remarks>This is a computed property that is true when <see cref="Visibility"/> is <see cref="TexturePackVisibility.Public"/>.</remarks>
        public bool AllowRandomSelection => Visibility == TexturePackVisibility.Public;

        /// <summary>
        /// Gets or sets the date and time when the texture pack was last updated (UTC).
        /// </summary>
        /// <remarks>This property is automatically updated whenever any other property changes.
        /// Setting this property directly also triggers a notification for <see cref="LastUpdatedLocal"/>.</remarks>
        public DateTimeOffset LastUpdatedUtc => _lastUpdatedUtc;

        /// <summary>
        /// Gets the date and time when the texture pack was last updated, converted to local time.
        /// </summary>
        /// <remarks>This is a computed property derived from <see cref="LastUpdatedUtc"/>.</remarks>
        public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

        /// <summary>
        /// Gets the collection of texture mappings contained within this texture pack.
        /// </summary>
        public UniqueTextureMappingCollection Mappings { get; } = [];

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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
        /// Copy constructor that creates a new <see cref="TexturePack"/> instance by copying the properties and mappings from an existing instance.
        /// </summary>
        /// <param name="source">The source <see cref="TexturePack"/> instance to copy from.</param>
        private TexturePack(TexturePack source, Guid? guid = null) : this(guid ?? source._uid)
        {
            _name = source._name;
            _description = source._description;
            _visibility = source._visibility;

            foreach (var mapping in source.Mappings)
                Mappings.Add(mapping.Clone());

            // Must be last: Mappings.Add fires PropertyChanged which overwrites _lastUpdatedUtc
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
        /// <returns>A new <see cref="TexturePack"/> instance that is a copy of the current instance with a new unique identifier (GUID).</returns>
        public TexturePack Copy() => new(this, Guid.NewGuid());

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates a property field and raises <see cref="PropertyChanged"/> if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">A reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property (automatically populated via <see cref="CallerMemberNameAttribute"/>).</param>
        /// <returns>True if the value changed and the property notification was raised; otherwise, false.</returns>
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Handles property change notifications to manage computed property updates and automatic timestamp/source tracking.
        /// </summary>
        /// <remarks>
        /// This method implements a dependency chain that ensures all computed properties and auto-tracked fields stay in sync:
        /// - When primary properties (Name, Description, Visibility) change, their computed dependents are notified.
        /// - When timestamp or source properties change, their computed dependents are notified.
        /// - When any user-editable property changes (not already auto-tracked), automatically updates LastUpdatedUtc and marks Source as Workspace.
        /// </remarks>
        /// <param name="sender">The object that raised the PropertyChanged event.</param>
        /// <param name="e">The event arguments containing the name of the property that changed.</param>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
            // Notify computed properties when their dependencies change
            case nameof(Visibility):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPrivate)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllowRandomSelection)));
                break;

            // Return after UTC timestamp changes to avoid running into infinite recursion
            case nameof(LastUpdatedUtc):
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedLocal)));
                return;

            case nameof(IsPrivate):
            case nameof(AllowRandomSelection):
            case nameof(LastUpdatedLocal):
                return;
            }

            // Auto-update LastUpdatedUtc and Source when any user-editable property changes
            _lastUpdatedUtc = DateTimeOffset.UtcNow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedUtc)));
        }

        private void OnPackMappingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            e.OldItems?.OfType<TextureMapping>().ToList().ForEach(mapping => mapping.PropertyChanged -= OnPropertyChanged);
            e.NewItems?.OfType<TextureMapping>().ToList().ForEach(mapping => mapping.PropertyChanged += OnPropertyChanged);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Mappings)));
        }

        #endregion

        #region Equality

        public bool Equals(TexturePack? other) => other is not null && _uid == other._uid;
        public override bool Equals(object? obj) => obj is TexturePack other && Equals(other);
        public override int GetHashCode() => _uid.GetHashCode();
        public static bool operator ==(TexturePack? left, TexturePack? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(TexturePack? left, TexturePack? right) => !(left == right);

        #endregion
    }
}
