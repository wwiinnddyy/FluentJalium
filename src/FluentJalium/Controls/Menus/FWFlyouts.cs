using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium flyout surface for arbitrary content.
/// </summary>
[ContentProperty("Content")]
public class FWFlyout : FlyoutBase, IFluentJaliumControl
{
    private FWFlyoutPresenter? _presenter;

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(FWFlyout),
            new PropertyMetadata(null, OnContentChanged));

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(nameof(ContentTemplate), typeof(DataTemplate), typeof(FWFlyout),
            new PropertyMetadata(null, OnContentChanged));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWFlyout),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    /// <summary>
    /// Gets or sets the content shown by the flyout.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display the flyout content.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the density preset for the flyout surface.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional presenter style for the flyout chrome.
    /// </summary>
    public Style? FlyoutPresenterStyle { get; set; }

    protected override Control CreatePresenter()
    {
        var presenter = new FWFlyoutPresenter(this);
        _presenter = presenter;
        var presenterStyle = FlyoutPresenterStyle ?? ResolvePresenterStyle();
        if (presenterStyle != null)
        {
            presenter.Style = presenterStyle;
        }

        SyncPresenter();

        if (!presenter.HasLocalValue(FWFlyoutPresenter.DensityProperty))
        {
            presenter.SetCurrentValue(FWFlyoutPresenter.DensityProperty, Density);
        }

        return presenter;
    }

    private static Style? ResolvePresenterStyle()
    {
        var app = Application.Current;
        if (app?.Resources != null &&
            app.Resources.TryGetValue("FWFlyoutPresenterStyle", out var resource) &&
            resource is Style style)
        {
            return style;
        }

        return null;
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFlyout flyout)
        {
            flyout.SyncPresenter();
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFlyout flyout &&
            e.NewValue is FWMenuDensity density &&
            flyout._presenter != null &&
            !flyout._presenter.HasLocalValue(FWFlyoutPresenter.DensityProperty))
        {
            flyout._presenter.SetCurrentValue(FWFlyoutPresenter.DensityProperty, density);
        }
    }

    private void SyncPresenter()
    {
        if (_presenter == null)
        {
            return;
        }

        _presenter.Content = Content;
        _presenter.ContentTemplate = ContentTemplate;
    }
}

/// <summary>
/// FluentJalium menu flyout surface.
/// </summary>
[ContentProperty("Items")]
public class FWMenuFlyout : FlyoutBase, IFluentJaliumControl
{
    private readonly ObservableCollection<Control> _items = new();
    private FWMenuFlyoutPresenter? _presenter;

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuFlyout),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuFlyout()
    {
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>
    /// Gets the menu controls shown by the flyout.
    /// </summary>
    public IList<Control> Items => _items;

    internal ObservableCollection<Control> ItemCollection => _items;

    /// <summary>
    /// Gets or sets an optional presenter style for the flyout chrome.
    /// </summary>
    public Style? MenuFlyoutPresenterStyle { get; set; }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    protected override Control CreatePresenter()
    {
        var presenter = new FWMenuFlyoutPresenter(this);
        _presenter = presenter;
        var presenterStyle = MenuFlyoutPresenterStyle ?? ResolvePresenterStyle();
        if (presenterStyle != null)
        {
            presenter.Style = presenterStyle;
        }

        if (!presenter.HasLocalValue(FWMenuFlyoutPresenter.DensityProperty))
        {
            presenter.SetCurrentValue(FWMenuFlyoutPresenter.DensityProperty, Density);
        }

        return presenter;
    }

    private static Style? ResolvePresenterStyle()
    {
        var app = Application.Current;
        if (app?.Resources != null &&
            app.Resources.TryGetValue("FWMenuFlyoutPresenterStyle", out var resource) &&
            resource is Style style)
        {
            return style;
        }

        return null;
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuFlyout flyout && e.NewValue is FWMenuDensity density)
        {
            flyout.ApplyDensityToItems(density);
            if (flyout._presenter != null &&
                !flyout._presenter.HasLocalValue(FWMenuFlyoutPresenter.DensityProperty))
            {
                flyout._presenter.SetCurrentValue(FWMenuFlyoutPresenter.DensityProperty, density);
            }
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems.OfType<Control>())
            {
                ApplyDensityToItem(item, Density);
            }
        }
    }

    private void ApplyDensityToItems(FWMenuDensity density)
    {
        foreach (var item in _items)
        {
            ApplyDensityToItem(item, density);
        }
    }

    private static void ApplyDensityToItem(Control item, FWMenuDensity density)
    {
        switch (item)
        {
            case FluentMenuFlyoutItemBase flyoutItem
                when !flyoutItem.HasLocalValue(FluentMenuFlyoutItemBase.DensityProperty):
                flyoutItem.SetCurrentValue(FluentMenuFlyoutItemBase.DensityProperty, density);
                break;
            case FluentToggleMenuFlyoutItemBase toggleItem
                when !toggleItem.HasLocalValue(FluentToggleMenuFlyoutItemBase.DensityProperty):
                toggleItem.SetCurrentValue(FluentToggleMenuFlyoutItemBase.DensityProperty, density);
                break;
            case FWMenuFlyoutSeparator separator
                when !separator.HasLocalValue(FWMenuFlyoutSeparator.DensityProperty):
                separator.SetCurrentValue(FWMenuFlyoutSeparator.DensityProperty, density);
                break;
        }
    }
}

/// <summary>
/// FluentJalium command bar flyout surface.
/// </summary>
public class FWCommandBarFlyout : FlyoutBase, IFluentJaliumControl
{
    private FWCommandBar? _commandBarPresenter;
    private bool _alwaysExpanded;

    public FWCommandBarFlyout()
    {
        PrimaryCommands.CollectionChanged += OnCommandsChanged;
        SecondaryCommands.CollectionChanged += OnCommandsChanged;
    }

    /// <summary>
    /// Gets the primary command elements shown in the command bar.
    /// </summary>
    public ObservableCollection<ICommandBarElement> PrimaryCommands { get; } = new();

    /// <summary>
    /// Gets the secondary command elements shown in the command bar.
    /// </summary>
    public ObservableCollection<ICommandBarElement> SecondaryCommands { get; } = new();

    /// <summary>
    /// Gets or sets whether secondary commands are expanded when the flyout opens.
    /// </summary>
    public bool AlwaysExpanded
    {
        get => _alwaysExpanded;
        set
        {
            if (_alwaysExpanded == value)
            {
                return;
            }

            _alwaysExpanded = value;
            if (_commandBarPresenter != null)
            {
                _commandBarPresenter.IsOpen = value;
            }
        }
    }

    protected override Control CreatePresenter()
    {
        _commandBarPresenter = new FWCommandBar();
        SyncCommandBarPresenter();
        return _commandBarPresenter;
    }

    private void OnCommandsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SyncCommandBarPresenter();
    }

    private void SyncCommandBarPresenter()
    {
        var commandBar = _commandBarPresenter;
        if (commandBar == null)
        {
            return;
        }

        commandBar.PrimaryCommands.Clear();
        foreach (var command in PrimaryCommands)
        {
            commandBar.PrimaryCommands.Add(command);
        }

        commandBar.SecondaryCommands.Clear();
        foreach (var command in SecondaryCommands)
        {
            commandBar.SecondaryCommands.Add(command);
        }

        commandBar.IsOpen = AlwaysExpanded;
    }
}

/// <summary>
/// FluentJalium submenu item for menu flyouts.
/// </summary>
[ContentProperty("Items")]
public class FWMenuFlyoutSubItem : FluentMenuFlyoutItemBase, IFluentJaliumControl
{
    private static readonly SolidColorBrush s_fallbackBackgroundBrush = new(Color.FromRgb(45, 45, 48));
    private static readonly SolidColorBrush s_fallbackBorderBrush = new(Color.FromRgb(67, 67, 70));
    private static readonly SolidColorBrush s_fallbackArrowBrush = new(Color.FromRgb(180, 180, 180));

    private readonly List<MenuFlyoutItem> _items = new();
    private Popup? _subPopup;
    private Border? _subPopupBorder;
    private FWSimpleMenuPopupScrollHost? _subPopupScrollHost;

    /// <summary>
    /// Gets the submenu items.
    /// </summary>
    public IList<MenuFlyoutItem> Items => _items;

    public FWMenuFlyoutSubItem()
    {
        AddHandler(MouseEnterEvent, new Jalium.UI.Input.MouseEventHandler(OnSubItemMouseEnter));
        AddHandler(MouseLeaveEvent, new Jalium.UI.Input.MouseEventHandler(OnSubItemMouseLeave));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
        {
            return;
        }

        var arrowBrush = ResolveBrush("OneTextSecondary", "TextSecondary", s_fallbackArrowBrush);
        const double arrowSize = 8.0;
        var arrowBounds = new Rect(
            Math.Max(0, RenderSize.Width - 16),
            Math.Max(0, (RenderSize.Height - arrowSize) / 2),
            arrowSize,
            arrowSize);
        ArrowIcons.DrawArrow(drawingContext, arrowBrush, arrowBounds, ArrowIcons.Direction.Right);
    }

    /// <summary>
    /// Shows the submenu popup.
    /// </summary>
    public void ShowSubMenu()
    {
        if (_items.Count == 0)
        {
            return;
        }

        CloseSiblingSubMenus();
        EnsureSubPopup();
        PopulateSubPopup();
        _subPopup!.IsOpen = true;
    }

    /// <summary>
    /// Hides the submenu popup.
    /// </summary>
    public void HideSubMenu()
    {
        CloseDescendantSubMenus();
        if (_subPopup != null)
        {
            _subPopup.IsOpen = false;
        }
    }

    protected override void InvokeItem()
    {
        ShowSubMenu();
        FocusFirstSubMenuItem();
    }

    protected override bool InvokeFromKeyboard()
    {
        ShowSubMenu();
        FocusFirstSubMenuItem();
        return true;
    }

    protected override void OnVisualParentChanged(Visual? oldParent)
    {
        base.OnVisualParentChanged(oldParent);

        if (VisualParent == null)
        {
            HideSubMenu();
        }
    }

    private void FocusFirstSubMenuItem()
    {
        if (_items.Count == 0)
        {
            return;
        }

        Dispatcher.BeginInvokeCritical(() =>
        {
            foreach (var item in _items)
            {
                if (!item.IsEnabled || item.Visibility != Visibility.Visible)
                {
                    continue;
                }

                if (item.Focus())
                {
                    return;
                }
            }
        });
    }

    private void EnsureSubPopup()
    {
        if (_subPopup != null)
        {
            return;
        }

        _subPopupScrollHost = new FWSimpleMenuPopupScrollHost();
        _subPopupBorder = new Border
        {
            Background = ResolveBrush("OnePopupBackground", "MenuFlyoutPresenterBackground", s_fallbackBackgroundBrush),
            BorderBrush = ResolveBrush("OnePopupBorder", "MenuFlyoutPresenterBorderBrush", s_fallbackBorderBrush),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(4),
            Child = _subPopupScrollHost,
            MinWidth = 160
        };

        _subPopup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Right,
            StaysOpen = false,
            IsLightDismissEnabled = true,
            ShouldConstrainToRootBounds = false,
            Child = _subPopupBorder
        };
        _subPopup.Closed += OnSubPopupClosed;
    }

    private void PopulateSubPopup()
    {
        var panel = _subPopupScrollHost?.ItemsPanel;
        if (panel == null)
        {
            return;
        }

        panel.Children.Clear();
        foreach (var item in _items)
        {
            ApplyDensityToSubItem(item);
            FWMenuFlyoutItemHost.AttachItemToPanel(panel, item);
        }
    }

    private void ApplyDensityToSubItem(MenuFlyoutItem item)
    {
        switch (item)
        {
            case FluentMenuFlyoutItemBase flyoutItem
                when !flyoutItem.HasLocalValue(FluentMenuFlyoutItemBase.DensityProperty):
                flyoutItem.SetCurrentValue(FluentMenuFlyoutItemBase.DensityProperty, Density);
                break;
            case FluentToggleMenuFlyoutItemBase toggleItem
                when !toggleItem.HasLocalValue(FluentToggleMenuFlyoutItemBase.DensityProperty):
                toggleItem.SetCurrentValue(FluentToggleMenuFlyoutItemBase.DensityProperty, Density);
                break;
        }
    }

    private void OnSubPopupClosed(object? sender, EventArgs e)
    {
        CloseDescendantSubMenus();
        _subPopupScrollHost?.ItemsPanel.Children.Clear();
    }

    private void OnSubItemMouseEnter(object sender, Jalium.UI.Input.MouseEventArgs e)
    {
        ShowSubMenu();
    }

    private void OnSubItemMouseLeave(object sender, Jalium.UI.Input.MouseEventArgs e)
    {
        // Keep submenu open while pointer moves from the item into its popup.
    }

    private Brush ResolveBrush(string primaryKey, string secondaryKey, Brush fallback)
    {
        if (TryFindResource(primaryKey) is Brush primary)
        {
            return primary;
        }

        if (TryFindResource(secondaryKey) is Brush secondary)
        {
            return secondary;
        }

        return fallback;
    }

    private void CloseSiblingSubMenus()
    {
        if (VisualParent is not Panel panel)
        {
            return;
        }

        foreach (var child in panel.Children)
        {
            if (child is FWMenuFlyoutSubItem sibling && !ReferenceEquals(sibling, this))
            {
                sibling.HideSubMenu();
            }
        }
    }

    private void CloseDescendantSubMenus()
    {
        foreach (var item in _items)
        {
            if (item is not FWMenuFlyoutSubItem childSubItem)
            {
                continue;
            }

            childSubItem.CloseDescendantSubMenus();
            if (childSubItem._subPopup != null)
            {
                childSubItem._subPopup.IsOpen = false;
            }
        }
    }
}

/// <summary>
/// Presents the content of a <see cref="FWFlyout"/>.
/// </summary>
public class FWFlyoutPresenter : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWFlyoutPresenter),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWFlyoutPresenter(FWFlyout flyout)
    {
        Content = flyout.Content;
        ContentTemplate = flyout.ContentTemplate;
        SetCurrentValue(DensityProperty, flyout.Density);
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWFlyoutPresenter presenter && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(presenter, density);
        }
    }

    private static void ApplyDensity(FWFlyoutPresenter presenter, FWMenuDensity density)
    {
        var (padding, cornerRadius) = FWMenuDensityMetrics.GetMenuFlyoutSurfaceMetrics(density);
        presenter.Padding = padding;
        presenter.CornerRadius = cornerRadius;
    }
}

public sealed class FWMenuFlyoutPresenter : Control
{
    private readonly FWMenuFlyout _flyout;
    private readonly FWSimpleMenuPopupScrollHost _scrollHost;

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWMenuDensity), typeof(FWMenuFlyoutPresenter),
            new PropertyMetadata(FWMenuDensity.Comfortable, OnDensityChanged));

    public FWMenuFlyoutPresenter(FWMenuFlyout flyout)
    {
        _flyout = flyout;
        _scrollHost = new FWSimpleMenuPopupScrollHost();
        SetCurrentValue(DensityProperty, flyout.Density);
        ApplyDensity(this, Density);

        _flyout.ItemCollection.CollectionChanged += OnFlyoutItemsChanged;
        RefreshItems();

        AddVisualChild(_scrollHost);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWMenuDensity Density
    {
        get => (FWMenuDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    public override int VisualChildrenCount => 1;

    public override Visual? GetVisualChild(int index)
    {
        if (index == 0)
        {
            return _scrollHost;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _scrollHost.Measure(availableSize);
        return _scrollHost.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _scrollHost.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    private void OnFlyoutItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshItems();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void RefreshItems()
    {
        _scrollHost.ItemsPanel.Children.Clear();
        foreach (var item in _flyout.Items)
        {
            FWMenuFlyoutItemHost.AttachItemToPanel(_scrollHost.ItemsPanel, item);
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWMenuFlyoutPresenter presenter && e.NewValue is FWMenuDensity density)
        {
            ApplyDensity(presenter, density);
        }
    }

    private static void ApplyDensity(FWMenuFlyoutPresenter presenter, FWMenuDensity density)
    {
        var (padding, cornerRadius) = FWMenuDensityMetrics.GetMenuFlyoutSurfaceMetrics(density);
        presenter.Padding = padding;
        presenter.CornerRadius = cornerRadius;
    }
}

internal static class FWMenuFlyoutItemHost
{
    public static void AttachItemToPanel(Panel panel, Control item)
    {
        if (item.VisualParent == panel)
        {
            return;
        }

        if (item.VisualParent is Panel oldPanel)
        {
            oldPanel.Children.Remove(item);
        }
        else if (item.VisualParent != null)
        {
            return;
        }

        panel.Children.Add(item);
    }
}

internal sealed class FWSimpleMenuPopupScrollHost : Control
{
    private readonly StackPanel _itemsPanel;
    private readonly ScrollViewer _scrollViewer;

    public FWSimpleMenuPopupScrollHost()
    {
        _itemsPanel = new StackPanel { Orientation = Orientation.Vertical };
        _scrollViewer = new ScrollViewer
        {
            Content = _itemsPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            IsScrollBarAutoHideEnabled = true,
            IsScrollInertiaEnabled = false
        };

        AddVisualChild(_scrollViewer);
    }

    public StackPanel ItemsPanel => _itemsPanel;

    public override int VisualChildrenCount => 1;

    public override Visual? GetVisualChild(int index)
    {
        if (index == 0)
        {
            return _scrollViewer;
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _scrollViewer.Measure(availableSize);
        return _scrollViewer.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _scrollViewer.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }
}
