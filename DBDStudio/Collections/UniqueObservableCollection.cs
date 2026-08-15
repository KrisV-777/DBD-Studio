using System.Collections.ObjectModel;

namespace DBDStudio.Collections
{
    /// <summary>
    /// Specifies the automatic sorting behaviour of a <see cref="UniqueObservableCollection{T}"/>.
    /// </summary>
    public enum SortBehaviour
    {
        /// <summary>
        /// Do not automatically sort the collection.
        /// </summary>
        None,

        /// <summary>
        /// Automatically sort the collection in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Automatically sort the collection in descending order.
        /// </summary>
        Descending
    }

    /// <summary>
    /// Represents an observable collection that enforces uniqueness according to a supplied
    /// <see cref="IEqualityComparer{T}"/> and optionally maintains its items in a sorted order.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    /// <param name="equalityComparer">The comparer used to determine equality.</param>
    /// <param name="sortComparer">The comparer used to determine ordering.</param>
    /// <param name="sortBehaviour">Whether the collection should automatically sort.</param>
    public class UniqueObservableCollection<T>(
        IEqualityComparer<T>? equalityComparer = null,
        IComparer<T>? sortComparer = null,
        SortBehaviour sortBehaviour = SortBehaviour.None) : ObservableCollection<T>
    {
        private readonly IEqualityComparer<T> _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        private readonly IComparer<T> _sortComparer = sortComparer ?? Comparer<T>.Default;

        /// <summary>
        /// Gets or sets the automatic sorting behaviour of the collection.
        /// </summary>
        /// <remarks>Changes do not immediately sort; call <see cref="Sort"/> explicitly if needed.</remarks>
        public SortBehaviour SortBehaviour { get; set; } = sortBehaviour;

        /// <summary>
        /// Gets the equality comparer used by the collection to determine uniqueness.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer => _equalityComparer;

        /// <summary>
        /// Gets the comparer used by the collection when sorting.
        /// </summary>
        public IComparer<T> SortComparer => _sortComparer;

        /// <summary>Inserts an item into the collection.</summary>
        /// <param name="index">The zero-based index at which the item should be inserted.</param>
        /// <param name="item">The item to insert.</param>
        /// <remarks>
        /// If an equal item exists, it is replaced. The collection is sorted if sorting is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
        protected override void InsertItem(int index, T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existingIndex = IndexOf(item);
            if (existingIndex >= 0) {
                base.SetItem(existingIndex, item);
            } else {
                base.InsertItem(index, item);
            }
            Sort();
        }

        /// <summary>
        /// Replaces the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to replace.</param>
        /// <param name="item">The replacement item.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an equal item already exists at a different index.</exception>
        protected override void SetItem(int index, T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            var existingIndex = IndexOf(item);

            if (existingIndex >= 0 && existingIndex != index) {
                throw new InvalidOperationException(
                    "An equal item already exists in the collection.");
            }

            base.SetItem(index, item);
            Sort();
        }

        /// <summary>
        /// Determines whether the collection contains an equal item.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>True if an equal item exists; otherwise false.</returns>
        public new bool Contains(T item) => IndexOf(item) >= 0;

        /// <summary>
        /// Searches for an item equal to the specified item and returns its index.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The zero-based index of the matching item, or -1 if not found.</returns>
        public new int IndexOf(T item)
        {
            for (var index = 0; index < Count; index++) {
                if (_equalityComparer.Equals(this[index], item)) {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Sorts the collection using the configured sort comparer and <see cref="SortBehaviour"/>.
        /// </summary>
        /// <remarks>No effect if <see cref="SortBehaviour"/> is None. Observers receive change notifications.</remarks>
        public void Sort()
        {
            if (SortBehaviour == SortBehaviour.None || Count < 2) {
                return;
            }

            var sortedItems = this.ToList();

            sortedItems.Sort(_sortComparer);
            if (SortBehaviour == SortBehaviour.Descending) {
                sortedItems.Reverse();
            }

            for (var index = 0; index < sortedItems.Count; index++) {
                if (!EqualityComparer<T>.Default.Equals(this[index], sortedItems[index])) {
                    base.SetItem(index, sortedItems[index]);
                }
            }
        }
    }
}
