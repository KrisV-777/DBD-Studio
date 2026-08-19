namespace DBDStudio.Models
{
    public abstract class DBDComponent : ModelBase
    {
        protected string _name = string.Empty;
        protected DateTimeOffset _lastUpdatedUtc = DateTimeOffset.UtcNow;

        #region Properties

        /// <summary>
        /// Gets the unique identifier (UID) of the component.
        /// </summary>
        public Guid Uid { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Gets the date and time when the component was last updated (UTC).
        /// </summary>
        /// <remarks>This property is automatically updated whenever any other property of the component changes.</remarks>
        public DateTimeOffset LastUpdatedUtc => _lastUpdatedUtc;

        /// <summary>
        /// Gets the date and time when the component was last updated, converted to local time.
        /// </summary>
        /// <remarks>This is a computed property derived from <see cref="LastUpdatedUtc"/>.</remarks>
        public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

        #endregion

        #region Methods

        public bool IsMoreRecentThan(DBDComponent? other) => other is not null && LastUpdatedUtc > other.LastUpdatedUtc;

        internal abstract DBDComponent Copy();

        internal abstract void Import(DBDComponent source);

        #endregion

        #region Constructors

        protected DBDComponent()
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(LastUpdatedUtc) && e.PropertyName != nameof(LastUpdatedLocal)) {
                    _lastUpdatedUtc = DateTimeOffset.UtcNow;
                    OnPropertyChanged(nameof(LastUpdatedUtc));
                    OnPropertyChanged(nameof(LastUpdatedLocal));
                }
            };
        }

        #endregion

        #region Equality

        public bool Equals(DBDComponent? other) => other is not null && Uid == other.Uid;
        public override bool Equals(object? obj) => obj is DBDComponent other && Equals(other);
        public static bool operator ==(DBDComponent? left, DBDComponent? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(DBDComponent? left, DBDComponent? right) => !(left == right);
        public override int GetHashCode() => Uid.GetHashCode();
        public int CompareTo(DBDComponent? other) => other is null ? 1 : Uid.CompareTo(other.Uid);

        #endregion
    }
}
