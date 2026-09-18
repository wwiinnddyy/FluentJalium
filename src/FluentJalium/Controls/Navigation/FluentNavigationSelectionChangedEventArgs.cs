namespace FluentJalium.Controls;

public sealed class FluentNavigationSelectionChangedEventArgs : EventArgs
{
    internal FluentNavigationSelectionChangedEventArgs(FluentNavigationItem? previousItem, FluentNavigationItem? selectedItem)
    {
        PreviousItem = previousItem;
        SelectedItem = selectedItem;
    }

    public FluentNavigationItem? PreviousItem { get; }
    public FluentNavigationItem? SelectedItem { get; }
}
