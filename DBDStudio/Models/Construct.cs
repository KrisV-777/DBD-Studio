using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using DBDStudio.Models.Component;

namespace DBDStudio.Models
{
    public enum ConstructState
    {
        None = -1,
        /// <summary>A construct that has no representation outside of the application.</summary>
        Ephemeral,
        /// <summary>A construct that has been loaded from a file (unedited).</summary>
        Primordial,
        /// <summary>A primordial construct that has been modified.</summary>
        Modified,
    }

    public abstract class Construct<T> : ModelBase, IEquatable<Construct<T>>, IComparable<Construct<T>>
        where T : DBDComponent
    {
        #region Properties

        public T Underlying { get; private set; }

        public required T? Primordial { get; init; }

        public Guid Uid => Underlying.Uid;

        public string Name => Underlying.Name;

        public DateTimeOffset LastUpdatedUtc => Underlying.LastUpdatedUtc;

        [JsonIgnore]
        public DateTimeOffset LastUpdatedLocal => LastUpdatedUtc.ToLocalTime();

        #endregion

        #region State

        private ConstructState? _stateCache = null;

        public virtual ConstructState State
        {
            get
            {
                if (Primordial is null) {
                    return ConstructState.Ephemeral;
                } else if (Underlying.IsMoreRecentThan(Primordial)) {
                    return ConstructState.Modified;
                } else {
                    return ConstructState.Primordial;
                }
            }
        }

        public bool Is(ConstructState state) => State == state;
        public bool IsPrimordialAny() => Is(ConstructState.Primordial) || Is(ConstructState.Modified);

        protected void RefreshStateCacheAndNotify()
        {
            if (_stateCache != State) {
                _stateCache = State;
                OnPropertyChanged(nameof(State));
            }
        }

        #endregion

        #region Constructors

        [SetsRequiredMembers]
        protected Construct(T underlying, bool isPrimordial = false)
        {
            Underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
            if (isPrimordial) {
                Primordial = underlying.Copy() as T
                    ?? throw new InvalidOperationException("Failed to clone the underlying component for primordial representation.");
            } else {
                Primordial = null;
            }

            _stateCache = State;

            Underlying.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(Underlying.LastUpdatedUtc) && e.PropertyName != nameof(Underlying.LastUpdatedLocal)) {
                    OnPropertyChanged(nameof(LastUpdatedUtc));
                    OnPropertyChanged(nameof(LastUpdatedLocal));
                }
                RefreshStateCacheAndNotify();
                OnPropertyChanged(e.PropertyName);
            };
        }

        public void Reset()
        {
            if (Primordial is null) {
                throw new InvalidOperationException("Cannot reset a construct that has no primordial representation.");
            }
            Underlying.Import(Primordial);
            Underlying.RestoreLastUpdatedUtc(Primordial.LastUpdatedUtc);
        }

        #endregion

        #region Equality

        public bool Equals(Construct<T>? other) => other is not null && Underlying == other.Underlying && Primordial == other.Primordial;
        public override bool Equals(object? obj) => obj is Construct<T> other && Equals(other);
        public static bool operator ==(Construct<T>? left, Construct<T>? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(Construct<T>? left, Construct<T>? right) => !(left == right);
        public override int GetHashCode() => Underlying.GetHashCode() ^ (Primordial?.GetHashCode() ?? 0);
        public int CompareTo(Construct<T>? other) => other is null ? 1 : Underlying.CompareTo(other.Underlying);

        #endregion
    }
}
