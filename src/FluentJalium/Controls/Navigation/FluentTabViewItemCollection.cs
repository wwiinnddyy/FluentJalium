using System.Collections.ObjectModel;

namespace FluentJalium.Controls;

/// <summary>
/// A tab view's ordered, mutable tabs. A tab belongs to exactly one view, and mutations keep the strip, the
/// selection and the body in step on the UI thread.
/// </summary>
public sealed class FluentTabViewItemCollection : Collection<FluentTabViewItem>
{
    private readonly FluentTabView _owner;

    internal FluentTabViewItemCollection(FluentTabView owner) => _owner = owner;

    protected override void InsertItem(int index, FluentTabViewItem item)
    {
        ValidateNewItem(item);
        base.InsertItem(index, item);
        _owner.OnItemsChanged(item);
    }

    protected override void SetItem(int index, FluentTabViewItem item)
    {
        var previous = this[index];
        if (ReferenceEquals(previous, item))
        {
            return;
        }

        ValidateNewItem(item);
        base.SetItem(index, item);
        _owner.DetachItem(previous);
        _owner.OnItemsChanged(item);
    }

    protected override void RemoveItem(int index)
    {
        var previous = this[index];
        base.RemoveItem(index);
        _owner.DetachItem(previous);
        _owner.RefreshStrip();
    }

    protected override void ClearItems()
    {
        var previous = this.ToArray();
        base.ClearItems();
        foreach (var item in previous)
        {
            _owner.DetachItem(item);
        }

        _owner.RefreshStrip();
    }

    private static void ValidateNewItem(FluentTabViewItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Owner != null || item.VisualParent != null)
        {
            throw new InvalidOperationException("Remove the tab from its existing parent before inserting it.");
        }
    }
}
