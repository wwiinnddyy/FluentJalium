using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Data;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Hands a control's ink to the <see cref="IconElement"/> it hosts.
///
/// An icon with no foreground of its own resolves one by walking up to an ancestor Control *while it draws*, and
/// the framework only invalidates it when its own property changes - so a live theme switch left the glyph painted
/// with the previous theme's ink while the label beside it re-tinted (docs/astra/audits/navigation.md §10, which
/// carries the screen readings). Writing the icon's own property is the only route in this runtime that both
/// changes the value and marks the visual dirty, and it leaves a readable value behind for a regression fact.
///
/// Two shapes use it: a control of ours that owns its icon calls <see cref="Apply"/> when the icon arrives, and a
/// template whose host is a framework control sets <c>fluent:IconInk.Source</c> on the element that wraps the icon,
/// which forwards to every icon under it once that host has been loaded.
/// </summary>
public static class IconInk
{
    public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
        "Source", typeof(Control), typeof(IconInk), new PropertyMetadata(null, OnSourceChanged));

    public static Control GetSource(DependencyObject target) => (Control)target.GetValue(SourceProperty)!;

    public static void SetSource(DependencyObject target, Control value) => target.SetValue(SourceProperty, value);

    /// <summary>
    /// Points <paramref name="icon"/>'s ink at <paramref name="carrier"/>'s. An icon that arrived with a foreground
    /// set on it keeps that: a local value outranks the hand-off, which is also how upstream lets an icon override
    /// its host.
    /// </summary>
    public static void Apply(Control carrier, IconElement icon)
    {
        var ownInk = icon.ReadLocalValue(IconElement.ForegroundProperty);
        if (ownInk is null || ReferenceEquals(ownInk, DependencyProperty.UnsetValue))
            BindingOperations.SetBinding(icon, IconElement.ForegroundProperty, new Binding("Foreground") { Source = carrier });
    }

    private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender is not FrameworkElement host || args.NewValue is not Control carrier) return;

        if (host.IsLoaded)
        {
            Forward(host, carrier);
            return;
        }

        RoutedEventHandler handler = null!;
        handler = (_, _) =>
        {
            host.Loaded -= handler;
            Forward(host, carrier);
        };
        host.Loaded += handler;
    }

    private static void Forward(DependencyObject root, Control carrier)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current is IconElement icon) Apply(carrier, icon);
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            {
                if (VisualTreeHelper.GetChild(current, index) is { } child) queue.Enqueue(child);
            }
        }
    }
}
