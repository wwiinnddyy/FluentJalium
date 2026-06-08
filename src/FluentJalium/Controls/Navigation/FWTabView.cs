using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using TabDock = Jalium.UI.Controls.Dock;

namespace FluentJalium.Controls;

/// <summary>
/// Describes how tab widths are calculated in <see cref="FWTabView"/>.
/// </summary>
public enum FWTabViewWidthMode
{
    Equal,
    SizeToContent,
    Compact
}

/// <summary>
/// Describes when close buttons are shown for <see cref="FWTabViewItem"/> instances.
/// </summary>
public enum FWTabViewCloseButtonOverlayMode
{
    Auto,
    OnPointerOver,
    Always,
    Never
}

/// <summary>
/// FluentJalium TabView control for document and workspace tabs.
/// </summary>
public class FWTabView : Selector, IFluentJaliumControl
{
    private Button? _addTabButton;

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(FWTabView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(FWTabView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(FWTabView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(FWTabView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TabStripPlacementProperty =
        DependencyProperty.Register(nameof(TabStripPlacement), typeof(TabDock), typeof(FWTabView),
            new PropertyMetadata(TabDock.Top, OnLayoutStateChanged), IsValidDock);

    public static readonly DependencyProperty TabWidthModeProperty =
        DependencyProperty.Register(nameof(TabWidthMode), typeof(FWTabViewWidthMode), typeof(FWTabView),
            new PropertyMetadata(FWTabViewWidthMode.Equal, OnLayoutStateChanged), IsValidWidthMode);

    public static readonly DependencyProperty CloseButtonOverlayModeProperty =
        DependencyProperty.Register(nameof(CloseButtonOverlayMode), typeof(FWTabViewCloseButtonOverlayMode), typeof(FWTabView),
            new PropertyMetadata(FWTabViewCloseButtonOverlayMode.Auto), IsValidCloseButtonOverlayMode);

    public static readonly DependencyProperty IsAddTabButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsAddTabButtonVisible), typeof(bool), typeof(FWTabView),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CanReorderTabsProperty =
        DependencyProperty.Register(nameof(CanReorderTabs), typeof(bool), typeof(FWTabView),
            new PropertyMetadata(false));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWTabView),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged), IsValidNavigationDensity);

    public static readonly DependencyProperty SelectedContentProperty =
        DependencyProperty.Register(nameof(SelectedContent), typeof(object), typeof(FWTabView),
            new PropertyMetadata(null));

    public static readonly RoutedEvent AddTabButtonClickEvent =
        EventManager.RegisterRoutedEvent(nameof(AddTabButtonClick), RoutingStrategy.Bubble,
            typeof(EventHandler<FWTabViewAddTabButtonClickEventArgs>), typeof(FWTabView));

    public static readonly RoutedEvent TabCloseRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(TabCloseRequested), RoutingStrategy.Bubble,
            typeof(EventHandler<FWTabViewTabCloseRequestedEventArgs>), typeof(FWTabView));

    public FWTabView()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public TabDock TabStripPlacement
    {
        get => (TabDock)GetValue(TabStripPlacementProperty)!;
        set => SetValue(TabStripPlacementProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWTabViewWidthMode TabWidthMode
    {
        get => (FWTabViewWidthMode)GetValue(TabWidthModeProperty)!;
        set => SetValue(TabWidthModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FWTabViewCloseButtonOverlayMode CloseButtonOverlayMode
    {
        get => (FWTabViewCloseButtonOverlayMode)GetValue(CloseButtonOverlayModeProperty)!;
        set => SetValue(CloseButtonOverlayModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsAddTabButtonVisible
    {
        get => (bool)GetValue(IsAddTabButtonVisibleProperty)!;
        set => SetValue(IsAddTabButtonVisibleProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool CanReorderTabs
    {
        get => (bool)GetValue(CanReorderTabsProperty)!;
        set => SetValue(CanReorderTabsProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? SelectedContent
    {
        get => GetValue(SelectedContentProperty);
        private set => SetValue(SelectedContentProperty, value);
    }

    public event EventHandler<FWTabViewAddTabButtonClickEventArgs> AddTabButtonClick
    {
        add => AddHandler(AddTabButtonClickEvent, value);
        remove => RemoveHandler(AddTabButtonClickEvent, value);
    }

    public event EventHandler<FWTabViewTabCloseRequestedEventArgs> TabCloseRequested
    {
        add => AddHandler(TabCloseRequestedEvent, value);
        remove => RemoveHandler(TabCloseRequestedEvent, value);
    }

    public void SelectTab(FWTabViewItem tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        SelectedItem = tab;
    }

    public bool RequestAddTab()
    {
        var args = new FWTabViewAddTabButtonClickEventArgs(AddTabButtonClickEvent, this);
        RaiseEvent(args);

        if (args.Cancel)
        {
            return false;
        }

        if (args.NewItem != null && ItemsSource == null)
        {
            Items.Add(args.NewItem);
            SelectedItem = args.NewItem;
        }

        return true;
    }

    public bool RequestCloseTab(FWTabViewItem tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        if (!tab.IsClosable)
        {
            return false;
        }

        var index = GetIndexOf(tab);
        if (index < 0 || ItemsSource != null)
        {
            return false;
        }

        var args = new FWTabViewTabCloseRequestedEventArgs(TabCloseRequestedEvent, this, tab, index);
        RaiseEvent(args);

        if (args.Cancel)
        {
            return false;
        }

        var wasSelected = ReferenceEquals(SelectedItem, tab);
        Items.Remove(tab);

        if (wasSelected)
        {
            var nextIndex = Math.Min(index, GetItemCount() - 1);
            SelectedIndex = nextIndex;
        }

        return true;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_addTabButton != null)
        {
            _addTabButton.Click -= OnAddTabButtonClick;
        }

        _addTabButton = GetTemplateChild("PART_AddTabButton") as Button;

        if (_addTabButton != null)
        {
            _addTabButton.Click += OnAddTabButtonClick;
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        SelectedContent = SelectedItem is FWTabViewItem tab ? tab.Content : SelectedItem;
        UpdateContainerSelection();
        base.OnSelectionChanged(e);
        InvalidateVisual();
    }

    protected override void UpdateContainerSelection()
    {
        foreach (var item in Items)
        {
            if (item is FWTabViewItem tab)
            {
                tab.IsSelected = ReferenceEquals(tab, SelectedItem);
            }
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabView tabView && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(tabView, density);
        }
    }

    private static void ApplyDensity(FWTabView tabView, FWNavigationDensity density)
    {
        var (minHeight, padding) = GetTabViewMetrics(density);
        tabView.MinHeight = minHeight;
        tabView.Padding = padding;
    }

    internal static (double MinHeight, Thickness Padding) GetTabViewMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (36.0, new Thickness(8, 4, 8, 4)),
            FWNavigationDensity.Spacious => (52.0, new Thickness(16, 10, 16, 10)),
            _ => (44.0, new Thickness(12, 6, 12, 6))
        };
    }

    private static void OnLayoutStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabView tabView)
        {
            tabView.InvalidateMeasure();
            tabView.InvalidateVisual();
        }
    }

    private void OnAddTabButtonClick(object sender, RoutedEventArgs e)
    {
        _ = RequestAddTab();
    }

    private static bool IsValidDock(object? value)
    {
        return value is TabDock dock && Enum.IsDefined(dock);
    }

    private static bool IsValidWidthMode(object? value)
    {
        return value is FWTabViewWidthMode mode && Enum.IsDefined(mode);
    }

    private static bool IsValidCloseButtonOverlayMode(object? value)
    {
        return value is FWTabViewCloseButtonOverlayMode mode && Enum.IsDefined(mode);
    }

    private static bool IsValidNavigationDensity(object? value)
    {
        return value is FWNavigationDensity density && Enum.IsDefined(density);
    }
}

/// <summary>
/// FluentJalium TabView item.
/// </summary>
public class FWTabViewItem : TabItem, IFluentJaliumControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FWTabViewItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(FWTabViewItem),
            new PropertyMetadata(true));

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty)!;
        set => SetValue(IsClosableProperty, value);
    }
}

/// <summary>
/// Event arguments for <see cref="FWTabView.AddTabButtonClick"/>.
/// </summary>
public class FWTabViewAddTabButtonClickEventArgs : RoutedEventArgs
{
    public FWTabViewAddTabButtonClickEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public FWTabViewItem? NewItem { get; set; }

    public bool Cancel { get; set; }
}

/// <summary>
/// Event arguments for <see cref="FWTabView.TabCloseRequested"/>.
/// </summary>
public class FWTabViewTabCloseRequestedEventArgs : RoutedEventArgs
{
    public FWTabViewTabCloseRequestedEventArgs(RoutedEvent routedEvent, object source, FWTabViewItem tab, int index) : base(routedEvent, source)
    {
        Tab = tab;
        Index = index;
    }

    public FWTabViewItem Tab { get; }

    public int Index { get; }

    public bool Cancel { get; set; }
}
