using System.Collections.Specialized;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// A path shown as crumbs, with the ones that do not fit collected behind an ellipsis: WinUI's
/// <c>BreadcrumbBar</c> shape, on the runtime's own items pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Own type because 26.10.9 exports no <c>BreadcrumbBar</c>, no <c>BreadcrumbBarItem</c> and none of the pieces
/// upstream's template is built from - <c>ItemsRepeater</c>, <c>StackLayout</c> and <c>Flyout</c> are all absent
/// (<c>spike/BreadcrumbProbe</c> pass A). What it does export is <see cref="ItemsControl"/>'s protected container
/// overrides, which is the pipeline <see cref="FluentRadioButtons"/> already runs, and upstream's bar exposes only
/// <c>ItemsSource</c> and <c>ItemTemplate</c> next to one event, so the base is a real match rather than a
/// stand-in.
/// </para>
/// <para>
/// The collapsing itself is <see cref="FluentBreadcrumbPanel"/>'s. This type keeps what a panel cannot: the
/// container-to-item map, because <c>ItemsControl.ContainerFromItem</c> does not exist here, and therefore the
/// only way to reach a crumb's visual is the list the container overrides fill.
/// </para>
/// </remarks>
public class FluentBreadcrumbBar : ItemsControl
{
    private const string DefaultStyleResourceKey = "DefaultBreadcrumbBarStyle";
    private const string EllipsisPartName = "PART_EllipsisButton";

    private Style? _appliedDefaultStyle;
    private Button? _ellipsis;
    private MenuFlyout? _ellipsisFlyout;
    private IReadOnlyList<FrameworkElement> _crumbs = [];
    private int _firstVisible;
    private bool _ellipsisIsRendered;

    /// <summary>Creates a breadcrumb bar and applies its named style when one is available.</summary>
    public FluentBreadcrumbBar()
    {
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();

        // The row is not a stock panel: ItemsPanelTemplate carries only a type, so the horizontal stacking and the
        // fit both have to come from a panel type of their own.
        ItemsPanel = new ItemsPanelTemplate { PanelType = typeof(FluentBreadcrumbPanel) };
    }

    /// <summary>Raised when a crumb other than the last one is activated, and for a collapsed crumb when its entry
    /// in the ellipsis list is activated.</summary>
    public event EventHandler<BreadcrumbBarItemClickedEventArgs>? ItemClicked;

    /// <summary>Gets the crumb that carries the item at <paramref name="index"/>, or <c>null</c> while that item
    /// has no container. Reads back in layout order, so index 0 is the leftmost crumb.</summary>
    public FluentBreadcrumbBarItem? ContainerFromIndex(int index)
    {
        foreach (var crumb in _crumbs)
        {
            if (crumb is FluentBreadcrumbBarItem item && item.BreadcrumbIndex == index)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>Gets the width the row of crumbs is judged against: this bar's own width, which is what the app
    /// gave it and is not disturbed by whether the ellipsis is standing in the way.</summary>
    internal double RowWidth => ActualWidth;

    /// <summary>Gets whether the ellipsis is currently rendered, which is the same question the panel answered for
    /// its last arrange pass.</summary>
    public bool IsEllipsisRendered => _ellipsisIsRendered;

    /// <summary>
    /// Gets the flyout the ellipsis opens, or <c>null</c> before the template is applied. Upstream reaches the same
    /// list through its crumb's private repeater (<c>BreadcrumbBar.xaml:113-152</c>), which has no equivalent here,
    /// so this is the only way an app or a test can read what the collapsed crumbs are - it is a lever, not an
    /// upstream API, and docs/astra/audits/breadcrumb-bar.md counts it as one.
    /// </summary>
    public MenuFlyout? EllipsisFlyout => _ellipsisFlyout;

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is FluentBreadcrumbBarItem;

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new FluentBreadcrumbBarItem();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not FluentBreadcrumbBarItem crumb)
        {
            return;
        }

        crumb.Content ??= item;
        crumb.CrumbClicked += OnCrumbClicked;
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is FluentBreadcrumbBarItem crumb)
        {
            crumb.CrumbClicked -= OnCrumbClicked;
            crumb.BreadcrumbIndex = -1;
        }
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs args)
    {
        base.OnItemsChanged(args);

        // Positions and the last-item state are written by the next arrange pass, which sees the new child order;
        // patching them here would be a second answer to the same question.
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_ellipsis is not null)
        {
            _ellipsis.Click -= OnEllipsisClick;
        }

        _ellipsis = GetTemplateChild(EllipsisPartName) as Button;
        _ellipsisFlyout ??= new MenuFlyout();
        if (_ellipsis is not null)
        {
            _ellipsis.Click += OnEllipsisClick;

            // Named the way upstream names it (ResourceArguments: "More"), because the label is the whole
            // announcement this control makes for the ellipsis.
            AutomationProperties.SetName(_ellipsis, "More");
        }
    }

    /// <summary>Reports the panel's arrange outcome: the index the visible crumbs start at, in child order.</summary>
    internal void ReportBreadcrumbOverflow(int firstVisible, IReadOnlyList<FrameworkElement> crumbs)
    {
        _crumbs = crumbs;
        _firstVisible = firstVisible;

        for (var index = 0; index < crumbs.Count; index++)
        {
            if (crumbs[index] is not FluentBreadcrumbBarItem crumb)
            {
                continue;
            }

            crumb.BreadcrumbIndex = index;
            crumb.IsLastItem = index == crumbs.Count - 1;
            if (index < firstVisible)
            {
                continue;
            }

            // Upstream re-indexes after every arrange pass and counts only the crumbs it rendered: the ellipsis is
            // never in the set, and neither is a collapsed crumb (BreadcrumbLayout.cpp:188-192,
            // BreadcrumbBar.cpp:399-424). Its item peer is a button named "breadcrumb bar item".
            AutomationProperties.SetName(crumb, "breadcrumb bar item");
            crumb.SetValue(AutomationProperties.PositionInSetProperty, index - firstVisible + 1);
            crumb.SetValue(AutomationProperties.SizeOfSetProperty, crumbs.Count - firstVisible);
        }

        UpdateEllipsis();
    }

    /// <summary>Gets the ellipsis's own size, measured on demand so the fit sees a real number even in the pass
    /// where it is still collapsed.</summary>
    internal Size EllipsisDesired()
    {
        if (_ellipsis is null)
        {
            return default;
        }

        _ellipsis.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return _ellipsis.DesiredSize;
    }

    private object? ItemAt(int index) => index >= 0 && index < Items.Count ? Items[index] : null;

    private void OnCrumbClicked(object? sender, EventArgs args)
    {
        if (sender is not FluentBreadcrumbBarItem crumb || crumb.BreadcrumbIndex < 0)
        {
            return;
        }

        RaiseItemClicked(crumb.BreadcrumbIndex);
    }

    private void OnEllipsisClick(object? sender, RoutedEventArgs args)
    {
        // Upstream's ellipsis click only opens the flyout and raises nothing (BreadcrumbBarItem.cpp:226-243).
        var flyout = _ellipsisFlyout;
        if (_firstVisible <= 0 || flyout is null || _ellipsis is null)
        {
            return;
        }

        flyout.Items.Clear();

        // Deepest first: upstream clones the hidden list in reverse order (BreadcrumbBarItem.cpp:287-296) and
        // hands each entry the index itemCount - index, so the topmost row is the crumb nearest the visible ones.
        for (var index = _firstVisible - 1; index >= 0; index--)
        {
            var position = index;
            var entry = new MenuFlyoutItem
            {
                Text = ItemAt(position)?.ToString() ?? string.Empty,
            };
            entry.Click += (_, _) => RaiseItemClicked(position);
            flyout.Items.Add(entry);
        }

        flyout.ShowAt(_ellipsis);
    }

    private void RaiseItemClicked(int index) =>
        ItemClicked?.Invoke(this, new BreadcrumbBarItemClickedEventArgs(index, ItemAt(index)));

    private void UpdateEllipsis()
    {
        if (_ellipsis is null)
        {
            return;
        }

        // The ellipsis renders exactly when the panel had to drop a crumb, so it never stands alone at the head of
        // a full row.
        var rendered = _firstVisible > 0;
        if (rendered == _ellipsisIsRendered)
        {
            return;
        }

        _ellipsisIsRendered = rendered;
        _ellipsis.Visibility = rendered ? Visibility.Visible : Visibility.Collapsed;

        // Revealing the ellipsis takes width away from the crumb row, so the row has to be fitted again with the
        // ellipsis's real size in it. One pass is what that costs: the decision above never depends on the answer,
        // so this cannot start a loop.
        InvalidateMeasure();
    }

    private void ApplyDefaultStyle()
    {
        // Same fallback as the item and every other own type here: an implicit style for a derived type is not
        // guaranteed to resolve, while an application-assigned style is never replaced.
        if ((Style is not null || _appliedDefaultStyle is not null) &&
            !ReferenceEquals(Style, _appliedDefaultStyle))
        {
            return;
        }

        if (_appliedDefaultStyle is null &&
            !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
        {
            return;
        }

        if (TryFindResource(DefaultStyleResourceKey) is not Style style)
        {
            return;
        }

        _appliedDefaultStyle = style;
        SetCurrentValue(StyleProperty, style);
    }
}
