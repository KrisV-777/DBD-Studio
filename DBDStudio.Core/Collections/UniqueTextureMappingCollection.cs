using System.Collections.ObjectModel;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Collections;

/// <summary>
/// A collection of TextureMapping objects that enforces uniqueness based on VanillaTexture.
/// Attempting to add a mapping with a duplicate VanillaTexture will throw an InvalidOperationException.
/// </summary>
public sealed class UniqueTextureMappingCollection : ObservableCollection<TextureMapping>
{
    protected override void InsertItem(int index, TextureMapping item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var oldIndex = IndexOf(item);
        if (oldIndex >= 0)
        {
            base.SetItem(oldIndex, item); // Update the existing item instead of adding a duplicate
        }
        else
        {
            base.InsertItem(index, item);
        }
    }

    protected override void SetItem(int index, TextureMapping item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Allow updating the same item at the same index, but prevent duplicates elsewhere
        var existingItem = this[index];
        if (!item.Equals(existingItem) && Contains(item))
            throw new InvalidOperationException($"A TextureMapping with VanillaTexture '{item.VanillaTexture}' already exists in the collection.");

        base.SetItem(index, item);
    }
}
