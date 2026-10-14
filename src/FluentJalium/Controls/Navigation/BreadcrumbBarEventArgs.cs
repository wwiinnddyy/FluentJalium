namespace FluentJalium.Controls;

/// <summary>Names the crumb the user activated and the position it holds in the bar.</summary>
public sealed class BreadcrumbBarItemClickedEventArgs : EventArgs
{
    internal BreadcrumbBarItemClickedEventArgs(int index, object? item)
    {
        Index = index;
        Item = item;
    }

    /// <summary>Gets the position of the activated item in the bar, or -1 while none is known.</summary>
    public int Index { get; }

    /// <summary>Gets the item the activated crumb carries.</summary>
    public object? Item { get; }
}
