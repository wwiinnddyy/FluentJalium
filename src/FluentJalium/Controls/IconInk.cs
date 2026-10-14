using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Data;

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
/// template whose host is somebody else's control names the two parts from markup - <c>fluent:IconInk.Carrier</c>
/// for the element whose ink to copy and <c>fluent:IconInk.Icon</c> for the icon property that holds the element.
/// Both are ordinary bindings, so an icon swapped later arrives here too. Nothing walks a tree to find out what a
/// control is showing: the structural gate in AstraGateTests keeps the visual-tree walk out of the library for the
/// reason AGENTS.md gives - it is how the old per-window repair loops started.
/// </summary>
public static class IconInk
{
    public static readonly DependencyProperty CarrierProperty = DependencyProperty.RegisterAttached(
        "Carrier", typeof(Control), typeof(IconInk), new PropertyMetadata(null, OnHandChanged));

    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached(
        "Icon", typeof(object), typeof(IconInk), new PropertyMetadata(null, OnHandChanged));

    public static Control GetCarrier(DependencyObject target) => (Control)target.GetValue(CarrierProperty)!;

    public static void SetCarrier(DependencyObject target, Control value) => target.SetValue(CarrierProperty, value);

    public static object? GetIcon(DependencyObject target) => target.GetValue(IconProperty);

    public static void SetIcon(DependencyObject target, object? value) => target.SetValue(IconProperty, value);

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

    private static void OnHandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        if (sender.GetValue(CarrierProperty) is Control carrier && sender.GetValue(IconProperty) is IconElement icon)
            Apply(carrier, icon);
    }
}
