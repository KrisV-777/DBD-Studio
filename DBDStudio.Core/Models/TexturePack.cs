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
    }

    /// <summary>
    /// Defines the origin or storage location of a texture pack.
    /// </summary>
    public enum TexturePackSource
    {
        /// <summary>The texture pack is stored in the workspace.</summary>
        Workspace,
        /// <summary>The texture pack is sourced from the mods folder.</summary>
        ModsFolder,
        /// <summary>The texture pack is sourced from the game data folder.</summary>
        GameDataFolder,
    }

    /// <summary>
    /// Represents a texture pack containing texture mappings and metadata.
    /// </summary>
    /// <remarks>
    /// This class automatically tracks modifications by updating <see cref="LastUpdatedUtc"/> and marking the <see cref="Source"/> as <see cref="TexturePackSource.Workspace"/>
    /// whenever any user-editable property changes. Computed properties (e.g., <see cref="IsPrivate"/>, <see cref="LastUpdatedLocal"/>) are automatically notified
    /// when their dependencies change, ensuring UI bindings remain in sync.
    /// </remarks>
    public sealed class TexturePack : INotifyPropertyChanged
    {
        #region Fields

        private string _name = string.Empty;
        private string _description = string.Empty;
        private TexturePackVisibility _visibility;
        private DateTimeOffset _lastUpdatedUtc = DateTimeOffset.MinValue;
        private TexturePackSource _source = TexturePackSource.Workspace;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the texture pack.
        /// </summary>
        /// <remarks>Setting this property automatically updates <see cref="LastUpdatedUtc"/> and marks the source as <see cref="TexturePackSource.Workspace"/>.</remarks>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the description of the texture pack.
        /// </summary>
        /// <remarks>Setting this property automatically updates <see cref="LastUpdatedUtc"/> and marks the source as <see cref="TexturePackSource.Workspace"/>.</remarks>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// Gets or sets the visibility level of the texture pack.
        /// </summary>
        /// <remarks>Setting this property automatically updates <see cref="LastUpdatedUtc"/> and marks the source as <see cref="TexturePackSource.Workspace"/>.
        /// Changing this value also triggers notifications for <see cref="IsPrivate"/> and <see cref="AllowRandomSelection"/>.</remarks>
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
                if (Visibility == newVisibility) {
                    return;
                }

                Visibility = newVisibility;
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
        public DateTimeOffset LastUpdatedUtc
        {
            get => _lastUpdatedUtc;
            set => SetProperty(ref _lastUpdatedUtc, value);
        }

        /// <summary>
        /// Gets the date and time when the texture pack was last updated, converted to local time.
        /// </summary>
        /// <remarks>This is a computed property derived from <see cref="LastUpdatedUtc"/>.</remarks>
        public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

        /// <summary>
        /// Gets or sets the source or origin of the texture pack.
        /// </summary>
        /// <remarks>This property is automatically set to <see cref="TexturePackSource.Workspace"/> whenever a user-editable property changes.
        /// Changing this value also triggers a notification for <see cref="SourceLabel"/>.</remarks>
        public TexturePackSource Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        /// <summary>
        /// Gets a human-readable label for the current <see cref="Source"/>.
        /// </summary>
        /// <remarks>This is a computed property that returns "Workspace", "Game Data", or "Unknown" based on the <see cref="Source"/> value.</remarks>
        public string SourceLabel => Source switch
        {
            TexturePackSource.Workspace => "Workspace",
            TexturePackSource.ModsFolder => "Mods",
            TexturePackSource.GameDataFolder => "Game Data",
            _ => "Unknown"
        };

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
        /// <remarks>Subscribes to the <see cref="PropertyChanged"/> event to handle automatic updates of timestamp and source tracking.</remarks>
        public TexturePack()
        {
            PropertyChanged += OnPropertyChanged;
        }

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
            switch (e.PropertyName)
            {
                // Notify computed properties when their dependencies change
                case nameof(Visibility):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPrivate)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllowRandomSelection)));
                    return;

                case nameof(LastUpdatedUtc):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedLocal)));
                    return;

                case nameof(Source):
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceLabel)));
                    return;

                // Skip computed properties to avoid triggering auto-update logic
                case nameof(IsPrivate):
                case nameof(AllowRandomSelection):
                case nameof(LastUpdatedLocal):
                case nameof(SourceLabel):
                    return;
            }

            // Auto-update LastUpdatedUtc and Source when any user-editable property changes
            _lastUpdatedUtc = DateTimeOffset.UtcNow;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastUpdatedUtc)));

            if (_source != TexturePackSource.Workspace)
            {
                _source = TexturePackSource.Workspace;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
            }
        }

        #endregion
    }
}
