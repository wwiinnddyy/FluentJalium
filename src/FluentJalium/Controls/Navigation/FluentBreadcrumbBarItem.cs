using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// One crumb of a <see cref="FluentBreadcrumbBar"/>: the content plus the chevron that separates it from the next
/// crumb, which is what upstream's own item peer is (<c>BreadcrumbBarItem.h</c>, a <c>ContentControl</c> with an
/// inline button, a last-item presenter and a chevron text block).
/// </summary>
/// <remarks>
/// The last crumb is not a button: upstream collapses <c>PART_ItemButton</c> and <c>PART_ChevronTextBlock</c> and
/// shows a plain presenter instead (<c>BreadcrumbBar.xaml:87-95</c>), which is why activating the last item raises
/// no click at all. That is a template fact here too, so the control does not have to filter it in code.
/// </remarks>
public class FluentBreadcrumbBarItem : ContentControl
{
    private const string ItemButtonPartName = "PART_ItemButton";

    private Button? _itemButton;

    /// <summary>Identifies the <see cref="IsLastItem"/> dependency property.</summary>
    public static readonly DependencyProperty IsLastItemProperty = DependencyProperty.Register(
        nameof(IsLastItem), typeof(bool), typeof(FluentBreadcrumbBarItem), new PropertyMetadata(false));

    /// <summary>Creates a crumb and aims its default style at this type rather than at ContentControl.</summary>
    public FluentBreadcrumbBarItem()
    {
        // Both lines are measured necessities rather than habit. A ContentControl-derived type whose Template is
        // set still paints its content and never expands the template until it opts in: the probe read
        // Template=ControlTemplate, LoadContent => a Grid holding PART_ItemButton, and a realized crumb whose only
        // visual child was a bare TextBlock (spike/BreadcrumbProbe mode style). DefaultStyleKey then aims the
        // lookup at this type, which is what lets Styles/BreadcrumbBar.jalxaml's implicit style be the one that
        // supplies the template - the same pair FluentTabViewItem runs.
        UseTemplateContentManagement();
        DefaultStyleKey = typeof(FluentBreadcrumbBarItem);
    }

    /// <summary>Raised when the crumb's own button is activated by pointer, keyboard or automation.</summary>
    internal event EventHandler? CrumbClicked;

    /// <summary>Gets or sets whether this crumb is the bar's last one, which paints it as plain text.</summary>
    public bool IsLastItem
    {
        get => (bool)GetValue(IsLastItemProperty)!;
        set => SetValue(IsLastItemProperty, value);
    }

    /// <summary>Gets the position this crumb holds among the bar's items, or -1 before the bar lays it out.</summary>
    internal int BreadcrumbIndex { get; set; } = -1;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_itemButton is not null)
        {
            _itemButton.Click -= OnItemButtonClick;
        }

        _itemButton = GetTemplateChild(ItemButtonPartName) as Button;
        if (_itemButton is not null)
        {
            _itemButton.Click += OnItemButtonClick;
        }
    }

    private void OnItemButtonClick(object? sender, RoutedEventArgs args) => CrumbClicked?.Invoke(this, EventArgs.Empty);
}
