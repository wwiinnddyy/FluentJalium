namespace FluentJalium.Controls;

/// <summary>Carries the tab the view moved away from and the one it moved to.</summary>
public sealed class FluentTabViewSelectionChangedEventArgs : EventArgs
{
    internal FluentTabViewSelectionChangedEventArgs(FluentTabViewItem? oldItem, FluentTabViewItem? newItem)
    {
        OldSelectedItem = oldItem;
        NewSelectedItem = newItem;
    }

    public FluentTabViewItem? OldSelectedItem { get; }

    public FluentTabViewItem? NewSelectedItem { get; }
}

/// <summary>Names the tab whose close button (or Delete key) the user invoked.</summary>
public sealed class FluentTabViewTabCloseRequestedEventArgs : EventArgs
{
    internal FluentTabViewTabCloseRequestedEventArgs(FluentTabViewItem item) => Item = item;

    public FluentTabViewItem Item { get; }
}
