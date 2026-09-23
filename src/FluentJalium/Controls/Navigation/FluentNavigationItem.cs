using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Data;

namespace FluentJalium.Controls;

/// <summary>A flat navigation destination. Keyboard focus does not imply selection.</summary>
public sealed class FluentNavigationItem : Button
{
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected), typeof(bool), typeof(FluentNavigationItem),
        new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(object), typeof(FluentNavigationItem),
        new PropertyMetadata(null, OnIconChanged));

    private static readonly DependencyPropertyKey IsCompactPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsCompact), typeof(bool), typeof(FluentNavigationItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsCompactProperty = IsCompactPropertyKey.DependencyProperty;

    private string? _automaticName;

    public FluentNavigationItem()
    {
        DefaultStyleKey = typeof(FluentNavigationItem);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty)!;
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>An optional Jalium icon or other small visual, displayed in the 40-DIP icon column.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsCompact => (bool)GetValue(IsCompactProperty)!;

    internal FluentNavigationView? Owner { get; set; }

    internal void SetCompact(bool value) => SetValue(IsCompactPropertyKey, value);

    protected override void OnContentChanged(object? oldContent, object? newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        var currentName = AutomationProperties.GetName(this);
        if (string.IsNullOrEmpty(currentName) || currentName == _automaticName)
        {
            _automaticName = newContent as string ?? string.Empty;
            AutomationProperties.SetName(this, _automaticName);
        }
    }

    private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var item = (FluentNavigationItem)sender;
        item.Owner?.OnItemSelectionChanged(item, (bool)args.NewValue!);
    }

    private static void OnIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var item = (FluentNavigationItem)sender;
        item.RemoveLogicalChild(args.OldValue);
        item.AddLogicalChild(args.NewValue);

        // An IconElement with no foreground of its own resolves one by walking to an ancestor Control *while it
        // draws*, and nothing invalidates it when that answer changes - so a live theme switch left the pane glyph
        // painted with the previous theme's ink while its label re-tinted (docs/astra/audits/navigation.md).
        // Forwarding this item's ink to the icon's own property is what makes it redraw; an icon that arrived with
        // a foreground set on it keeps that, because a local value outranks the hand-off.
        if (args.NewValue is not IconElement icon) return;
        var ownInk = icon.ReadLocalValue(IconElement.ForegroundProperty);
        if (ownInk is null || ReferenceEquals(ownInk, DependencyProperty.UnsetValue))
            BindingOperations.SetBinding(icon, IconElement.ForegroundProperty, new Binding(nameof(Foreground)) { Source = item });
    }
}
