using System.Collections.ObjectModel;

namespace DBDStudio.Utility
{
    public static class CollectionFilter
    {
        public static void ApplyTextFilter<T>(
            ObservableCollection<T> target,
            IEnumerable<T> source,
            string? searchText,
            params Func<T, string?>[] searchableFields)
        {
            target.Clear();

            var terms = searchText?.Trim();
            var filtered = string.IsNullOrWhiteSpace(terms)
                ? source
                : source.Where(item => searchableFields.Any(field =>
                    (field(item) ?? string.Empty).Contains(terms, StringComparison.OrdinalIgnoreCase)));

            foreach (var item in filtered) {
                target.Add(item);
            }
        }
    }
}
