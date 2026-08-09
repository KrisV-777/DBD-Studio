
using DBDStudio.Core.Interfaces;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DBDStudio.Core.Models.Textures
{
    /// <summary>
    /// Wraps a <see cref="TexturePack"/> instance, providing additional metadata and state information for use in the application.
    /// </summary>
    public sealed class TexturePackData : ModelBase, IRenderedTexturePack, INotifyPropertyChanged, IEquatable<TexturePackData>, IComparable<TexturePackData>
    {
        #region Properties

        /// <summary>
        /// Gets the underlying <see cref="TexturePack"/> instance that this <see cref="TexturePackData"/> wraps.
        /// </summary>
        public TexturePack Underlying { get; }

        /// <summary>
        /// Gets the primordial (original) <see cref="TexturePack"/> instance that this <see cref="TexturePackData"/> was derived from, if any.
        /// </summary>
        /// <remarks>See <see cref="TexturePackState"/> for details on how this property affects the state of the texture pack.</remarks>
        public TexturePack? Primordial { get; }

        /// <summary>
        /// Gets the unique identifier (GUID) of the texture pack.
        /// </summary>
        public Guid Uid => Underlying.Uid;

        /// <summary>
        /// Gets the name of the texture pack.
        /// </summary>
        public string Name => Underlying.Name;

        /// <summary>
        /// Gets the description of the texture pack.
        /// </summary>
        public string Description => Underlying.Description;

        /// <summary>
        /// Gets a value indicating whether the texture pack is public (accessible to random selection).
        /// </summary>
        public bool IsPrivate => Underlying.IsPrivate;

        /// <summary>
        /// Gets or sets the date and time when the texture pack was last updated (UTC).
        /// </summary>
        /// <remarks>This property is automatically updated whenever any other property changes.
        /// Setting this property directly also triggers a notification for <see cref="LastUpdatedLocal"/>.</remarks>
        public DateTimeOffset LastUpdatedUtc => Underlying.LastUpdatedUtc;

        /// <summary>
        /// Gets the date and time when the texture pack was last updated, converted to local time.
        /// </summary>
        /// <remarks>This is a computed property derived from <see cref="LastUpdatedUtc"/>.</remarks>
        public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

        /// <summary>
        /// Gets a read-only collection of texture mappings contained in the underlying <see cref="TexturePack"/>.
        /// </summary>
        public ReadOnlyCollection<TextureMapping> Mappings => Underlying.Mappings.AsReadOnly();

        /// <summary>
        /// Gets the number of texture mappings contained in the underlying <see cref="TexturePack"/>.
        /// </summary>
        public int NumMappings => Underlying.Mappings.Count;

        /// <summary>
        /// Gets the current state of the texture pack, indicating its relationship to its primordial version, if any.
        /// </summary>
        public TexturePackState State
        {
            get
            {
                if (Primordial is null) {
                    return TexturePackState.Ephemeral;
                } else if (Underlying.IsMoreRecentThan(Primordial)) {
                    return TexturePackState.Modified;
                } else {
                    return TexturePackState.Primordial;
                }
            }
        }

        #endregion

        #region Methods

        public bool Is(TexturePackState state) => State == state;
        public bool IsPrimordial() => Is(TexturePackState.Primordial) || Is(TexturePackState.Modified);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePackData"/> class, wrapping the specified <see cref="TexturePack"/> and optionally associating it with a primordial version.
        /// </summary>
        /// <param name="pack">The <see cref="TexturePack"/> instance to wrap.</param>
        /// <param name="primordialPack">The primordial (original) <see cref="TexturePack"/> instance, if any.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pack"/> is null.</exception>
        public TexturePackData(TexturePack pack, TexturePack? primordialPack = null)
        {
            System.Diagnostics.Debug.Assert(primordialPack is null || pack.Uid == primordialPack.Uid);

            Underlying = pack ?? throw new ArgumentNullException(nameof(pack));
            Primordial = primordialPack;
            Underlying.PropertyChanged += OnUnderlyingTexturePackUpdated;
        }

        /// <summary>
        /// Creates an ephemeral copy of the current <see cref="TexturePackData"/> instance, including a copy of the underlying <see cref="TexturePack"/>.
        /// </summary>
        /// <returns>A new <see cref="TexturePackData"/> instance that is a copy of the current instance.</returns>
        public IRenderedTexturePack Copy() => new TexturePackData(Underlying.Copy());

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes on the underlying <see cref="TexturePack"/> instance, allowing subscribers to react to changes in the texture pack's data.
        /// </summary>
        /// <param name="sender">The source of the event, typically the underlying <see cref="TexturePack"/> instance.</param>
        /// <param name="e">An object that contains the event data.</param>
        private void OnUnderlyingTexturePackUpdated(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);

            switch (e.PropertyName) {
            case nameof(TexturePack.LastUpdatedUtc):
                OnPropertyChanged(nameof(LastUpdatedLocal));
                break;
            }
        }

        #endregion

        #region Equality

        public bool Equals(TexturePackData? other) => other is not null && Underlying.Uid == other.Underlying.Uid;
        public override bool Equals(object? obj) => obj is TexturePackData other && Equals(other);
        public static bool operator ==(TexturePackData? left, TexturePackData? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(TexturePackData? left, TexturePackData? right) => !(left == right);
        public override int GetHashCode() => Underlying.Uid.GetHashCode();
        public int CompareTo(TexturePackData? other) => other is null ? 1 : Underlying.Uid.CompareTo(other.Underlying.Uid);

        public static bool operator <(TexturePackData? left, TexturePackData? right) => left is null ? right is not null : left.CompareTo(right) < 0;
        public static bool operator >(TexturePackData? left, TexturePackData? right) => left is not null && left.CompareTo(right) > 0;
        public static bool operator <=(TexturePackData? left, TexturePackData? right) => !(left > right);
        public static bool operator >=(TexturePackData? left, TexturePackData? right) => !(left < right);

        #endregion
    }
}