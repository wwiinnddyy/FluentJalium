using System.Collections.ObjectModel;

namespace FluentJalium.Controls;

/// <summary>
/// A navigation view's ordered, mutable destinations. An item belongs to exactly one collection.
/// Mutations synchronously update the pane and selection on the UI thread.
/// </summary>
public sealed class FluentNavigationItemCollection : Collection<FluentNavigationItem>
{
    private readonly FluentNavigationView _owner;

    internal FluentNavigationItemCollection(FluentNavigationView owner) => _owner = owner;

    protected override void InsertItem(int index, FluentNavigationItem item)
    {
        ValidateNewItem(item);
        base.InsertItem(index, item);
        _owner.OnItemsChanged(null, item);
    }

    protected override void SetItem(int index, FluentNavigationItem item)
    {
        var previous = this[index];
        if (ReferenceEquals(previous, item)) return;
        ValidateNewItem(item);
        base.SetItem(index, item);
        _owner.OnItemsChanged(previous, item);
    }

    protected override void RemoveItem(int index)
    {
        var previous = this[index];
        base.RemoveItem(index);
        _owner.OnItemsChanged(previous, null);
    }

    protected override void ClearItems()
    {
        var previous = this.ToArray();
        base.ClearItems();
        foreach (var item in previous) _owner.DetachItem(item);
        _owner.RefreshItems();
    }

    private static void ValidateNewItem(FluentNavigationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Owner != null || item.VisualParent != null)
            throw new InvalidOperationException("Remove the navigation item from its existing parent before inserting it.");
    }
}
