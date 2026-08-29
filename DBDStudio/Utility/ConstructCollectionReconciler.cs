using System.Collections.ObjectModel;
using System.Diagnostics;
using DBDStudio.Models;

namespace DBDStudio.Utility
{
    public static class ConstructCollectionReconciler
    {
        public static void ReconcileByUid<TComponent, TConstruct>(
            ObservableCollection<TConstruct> constructs,
            IEnumerable<TComponent> previousComponents,
            Func<TConstruct, Guid> uidSelector,
            Func<TConstruct, TComponent> currentSelector,
            Func<TConstruct, TComponent?> primordialSelector,
            Func<TComponent, bool, TConstruct> constructFactory)
            where TComponent : DBDComponent
        {
            foreach (var previous in previousComponents) {
                var current = constructs.FirstOrDefault(construct => uidSelector(construct).Equals(previous.Uid));
                if (current is null) {
                    constructs.Add(constructFactory(previous, false));
                    continue;
                }

                var primordial = primordialSelector(current);
                Debug.Assert(primordial is not null);
                Debug.Assert(primordial.LastUpdatedUtc == currentSelector(current).LastUpdatedUtc);

                if (previous.IsMoreRecentThan(primordial)) {
                    constructs.Remove(current);
                    constructs.Add(constructFactory(previous, true));
                }
            }
        }
    }
}
