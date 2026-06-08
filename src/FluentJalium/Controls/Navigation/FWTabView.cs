using System.Collections.Specialized;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Controls.Themes;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Media;
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
            new PropertyMetadata(FWTabViewCloseButtonOverlayMode.Auto, OnCloseButtonOverlayModeChanged), IsValidCloseButtonOverlayMode);

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
        Items.CollectionChanged += OnItemsChanged;
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

    public FWTabViewDiagnostics GetDiagnostics()
    {
        return new FWTabViewDiagnostics(
            GetItemCount(),
            SelectedIndex,
            SelectedItem,
            SelectedItem is FWTabViewItem tab ? tab.Header?.ToString() ?? string.Empty : SelectedItem?.ToString() ?? string.Empty,
            SelectedContent,
            SelectedItem != null,
            Density,
            TabStripPlacement,
            TabWidthMode,
            CloseButtonOverlayMode,
            IsAddTabButtonVisible,
            CanReorderTabs,
            MinHeight,
            Padding);
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
                tab.SetOwnerTabView(this);
                tab.IsSelected = ReferenceEquals(tab, SelectedItem);
                tab.UpdateCloseButtonState();
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

    private static void OnCloseButtonOverlayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabView tabView)
        {
            tabView.UpdateTabItemCloseButtonState();
            tabView.InvalidateMeasure();
            tabView.InvalidateVisual();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is FWTabViewItem tab && ReferenceEquals(tab.OwnerTabView, this))
                {
                    tab.SetOwnerTabView(null);
                }
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is FWTabViewItem tab)
                {
                    tab.SetOwnerTabView(this);
                }
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            UpdateTabItemCloseButtonState();
        }
    }

    private void UpdateTabItemCloseButtonState()
    {
        foreach (var item in Items)
        {
            if (item is FWTabViewItem tab)
            {
                tab.SetOwnerTabView(this);
                tab.UpdateCloseButtonState();
            }
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
/// Snapshot of tab view state for Gallery diagnostics and tests.
/// </summary>
public readonly record struct FWTabViewDiagnostics(
    int ItemCount,
    int SelectedIndex,
    object? SelectedItem,
    string SelectedHeader,
    object? SelectedContent,
    bool HasSelection,
    FWNavigationDensity Density,
    TabDock TabStripPlacement,
    FWTabViewWidthMode TabWidthMode,
    FWTabViewCloseButtonOverlayMode CloseButtonOverlayMode,
    bool IsAddTabButtonVisible,
    bool CanReorderTabs,
    double MinHeight,
    Thickness Padding);

/// <summary>
/// FluentJalium TabView item.
/// </summary>
public class FWTabViewItem : TabItem, IFluentJaliumControl
{
    private const double DefaultCloseButtonWidth = 32.0;
    private const double DefaultCloseGlyphSize = 10.0;

    private static readonly SolidColorBrush s_selectedBackgroundBrush = new(ThemeColors.TabItemSelectedBackground);
    private static readonly SolidColorBrush s_hoverBackgroundBrush = new(ThemeColors.TabItemHoverBackground);
    private static readonly SolidColorBrush s_transparentBrush = new(Color.Transparent);
    private static readonly SolidColorBrush s_textPrimaryBrush = new(ThemeColors.TextPrimary);
    private static readonly SolidColorBrush s_textSecondaryBrush = new(ThemeColors.TextSecondary);
    private static readonly SolidColorBrush s_indicatorBrush = new(ThemeColors.TabItemIndicator);
    private static readonly SolidColorBrush s_closeButtonHoverBrush = new(Color.FromArgb(28, 255, 255, 255));

    private bool _isPointerOverCloseButton;
    private Pen? _closeGlyphPen;
    private Brush? _closeGlyphPenBrush;
    private FWTabView? _ownerTabView;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FWTabViewItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(FWTabViewItem),
            new PropertyMetadata(true, OnCloseStateChanged));

    public static readonly DependencyProperty CloseButtonWidthProperty =
        DependencyProperty.Register(nameof(CloseButtonWidth), typeof(double), typeof(FWTabViewItem),
            new PropertyMetadata(DefaultCloseButtonWidth, OnCloseStateChanged), IsValidCloseButtonWidth);

    private static readonly DependencyPropertyKey IsCloseButtonVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCloseButtonVisible), typeof(bool), typeof(FWTabViewItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsCloseButtonVisibleProperty = IsCloseButtonVisiblePropertyKey.DependencyProperty;

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

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double CloseButtonWidth
    {
        get => (double)GetValue(CloseButtonWidthProperty)!;
        set => SetValue(CloseButtonWidthProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsCloseButtonVisible => (bool)GetValue(IsCloseButtonVisibleProperty)!;

    internal FWTabView? OwnerTabView => _ownerTabView;

    public bool RequestClose()
    {
        return ResolveOwnerTabView()?.RequestCloseTab(this) == true;
    }

    public Rect GetCloseButtonBounds(Size availableSize)
    {
        if (!ShouldReserveCloseButtonSpace() ||
            !double.IsFinite(availableSize.Width) ||
            !double.IsFinite(availableSize.Height) ||
            availableSize.Width <= 0 ||
            availableSize.Height <= 0)
        {
            return Rect.Empty;
        }

        var closeButtonWidth = Math.Min(CloseButtonWidth, availableSize.Width);
        var closeButtonHeight = Math.Min(closeButtonWidth, availableSize.Height);
        var closeX = Math.Max(0, availableSize.Width - closeButtonWidth);
        var closeY = Math.Max(0, (availableSize.Height - closeButtonHeight) / 2.0);

        return new Rect(closeX, closeY, closeButtonWidth, closeButtonHeight);
    }

    public bool IsPointInCloseButton(Point point)
    {
        return IsPointInCloseButton(point, new Size(ActualWidth, ActualHeight));
    }

    public bool IsPointInCloseButton(Point point, Size availableSize)
    {
        return IsCloseButtonVisible && GetCloseButtonBounds(availableSize).Contains(point);
    }

    internal void SetOwnerTabView(FWTabView? owner)
    {
        if (ReferenceEquals(_ownerTabView, owner))
        {
            return;
        }

        _ownerTabView = owner;
        UpdateCloseButtonState();
    }

    internal void UpdateCloseButtonState()
    {
        var isVisible = ResolveCloseButtonVisibility();
        if (IsCloseButtonVisible != isVisible)
        {
            SetValue(IsCloseButtonVisiblePropertyKey.DependencyProperty, isVisible);
        }

        InvalidateMeasure();
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var measured = base.MeasureOverride(availableSize);
        if (!ShouldReserveCloseButtonSpace())
        {
            return measured;
        }

        var reservedWidth = CloseButtonWidth;
        var width = measured.Width + reservedWidth;
        if (double.IsFinite(availableSize.Width))
        {
            width = Math.Min(width, availableSize.Width);
        }

        var height = Math.Max(measured.Height, Math.Min(reservedWidth, double.IsFinite(availableSize.Height) ? availableSize.Height : reservedWidth));
        return new Size(width, height);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        if (e.Handled || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        if (IsPointInCloseButton(e.GetPosition(this)) && RequestClose())
        {
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        UpdateCloseButtonHoverState(e.GetPosition(this));
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        UpdateCloseButtonHoverState(null);
    }

    protected override void OnIsMouseOverChanged(bool oldValue, bool newValue)
    {
        base.OnIsMouseOverChanged(oldValue, newValue);
        UpdateCloseButtonState();
    }

    protected override void OnRender(DrawingContext drawingContextObj)
    {
        var dc = drawingContextObj;
        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);

        Brush backgroundBrush;
        if (IsSelected)
        {
            backgroundBrush = ResolveSelectedBackground();
        }
        else if (IsMouseOver)
        {
            backgroundBrush = ResolveHoverBackground();
        }
        else
        {
            backgroundBrush = Background ?? s_transparentBrush;
        }

        dc.DrawRectangle(backgroundBrush, null, bounds);
        DrawHeader(dc, bounds);

        if (IsCloseButtonVisible)
        {
            DrawCloseButton(dc, GetCloseButtonBounds(bounds.Size));
        }

        if (IsSelected)
        {
            var indicatorHeight = Math.Max(0, IndicatorHeight);
            if (indicatorHeight > 0)
            {
                var indicatorRect = new Rect(0, ActualHeight - indicatorHeight, ActualWidth, indicatorHeight);
                dc.DrawRectangle(ResolveIndicatorBrush(), null, indicatorRect);
            }
        }
    }

    private void DrawHeader(DrawingContext dc, Rect bounds)
    {
        var headerText = Header?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(headerText))
        {
            return;
        }

        var textBrush = IsSelected || IsMouseOver ? ResolvePrimaryTextBrush() : ResolveSecondaryTextBrush();
        var fontSize = FontSize > 0 ? FontSize : 13;
        var fontFamily = !string.IsNullOrEmpty(FontFamily) ? FontFamily : FrameworkElement.DefaultFontFamilyName;
        var text = new FormattedText(headerText, fontFamily, fontSize)
        {
            Foreground = textBrush
        };
        TextMeasurement.MeasureText(text);

        var reservedWidth = ShouldReserveCloseButtonSpace() ? CloseButtonWidth : 0;
        var contentWidth = Math.Max(0, bounds.Width - reservedWidth);
        var textX = Math.Max(0, (contentWidth - text.Width) / 2.0);
        var textY = Math.Max(0, (bounds.Height - text.Height) / 2.0);

        dc.DrawText(text, new Point(textX, textY));
    }

    private void DrawCloseButton(DrawingContext dc, Rect closeBounds)
    {
        if (closeBounds.IsEmpty)
        {
            return;
        }

        if (_isPointerOverCloseButton)
        {
            var hoverRect = new Rect(
                closeBounds.X + 4,
                closeBounds.Y + 4,
                Math.Max(0, closeBounds.Width - 8),
                Math.Max(0, closeBounds.Height - 8));
            dc.DrawRoundedRectangle(ResolveCloseButtonHoverBrush(), null, hoverRect, new CornerRadius(4));
        }

        var glyphBrush = ResolvePrimaryTextBrush();
        if (_closeGlyphPen == null || !ReferenceEquals(_closeGlyphPenBrush, glyphBrush))
        {
            _closeGlyphPenBrush = glyphBrush;
            _closeGlyphPen = new Pen(glyphBrush, 1.5);
        }

        var glyphSize = Math.Min(DefaultCloseGlyphSize, Math.Min(closeBounds.Width, closeBounds.Height) - 10);
        if (glyphSize <= 0)
        {
            return;
        }

        var center = closeBounds.Center;
        var half = glyphSize / 2.0;
        dc.DrawLine(_closeGlyphPen, new Point(center.X - half, center.Y - half), new Point(center.X + half, center.Y + half));
        dc.DrawLine(_closeGlyphPen, new Point(center.X + half, center.Y - half), new Point(center.X - half, center.Y + half));
    }

    private void UpdateCloseButtonHoverState(Point? pointerPosition)
    {
        var isPointerOverCloseButton = pointerPosition.HasValue && IsPointInCloseButton(pointerPosition.Value);
        if (_isPointerOverCloseButton == isPointerOverCloseButton)
        {
            return;
        }

        _isPointerOverCloseButton = isPointerOverCloseButton;
        InvalidateVisual();
    }

    private bool ResolveCloseButtonVisibility()
    {
        if (!IsClosable)
        {
            return false;
        }

        return ResolveCloseButtonOverlayMode() switch
        {
            FWTabViewCloseButtonOverlayMode.Always => true,
            FWTabViewCloseButtonOverlayMode.Never => false,
            FWTabViewCloseButtonOverlayMode.OnPointerOver => IsMouseOver,
            _ => IsSelected || IsMouseOver
        };
    }

    private bool ShouldReserveCloseButtonSpace()
    {
        return IsClosable &&
            CloseButtonWidth > 0 &&
            ResolveCloseButtonOverlayMode() != FWTabViewCloseButtonOverlayMode.Never;
    }

    private FWTabViewCloseButtonOverlayMode ResolveCloseButtonOverlayMode()
    {
        return ResolveOwnerTabView()?.CloseButtonOverlayMode ?? FWTabViewCloseButtonOverlayMode.Auto;
    }

    private FWTabView? ResolveOwnerTabView()
    {
        if (_ownerTabView != null)
        {
            return _ownerTabView;
        }

        for (var current = VisualParent; current != null; current = current.VisualParent)
        {
            if (current is FWTabView tabView)
            {
                _ownerTabView = tabView;
                return tabView;
            }
        }

        return null;
    }

    private Brush ResolveSelectedBackground()
    {
        return SelectedBackground
            ?? TryFindResource("TabItemSelectedBackground") as Brush
            ?? s_selectedBackgroundBrush;
    }

    private Brush ResolveHoverBackground()
    {
        return HoverBackground
            ?? TryFindResource("TabItemHoverBackground") as Brush
            ?? s_hoverBackgroundBrush;
    }

    private Brush ResolvePrimaryTextBrush()
    {
        return TryFindResource("TextPrimary") as Brush
            ?? Foreground
            ?? s_textPrimaryBrush;
    }

    private Brush ResolveSecondaryTextBrush()
    {
        if (HasLocalValue(Control.ForegroundProperty) && Foreground != null)
        {
            return Foreground;
        }

        return TryFindResource("TextSecondary") as Brush
            ?? Foreground
            ?? s_textSecondaryBrush;
    }

    private Brush ResolveIndicatorBrush()
    {
        return IndicatorBrush
            ?? TryFindResource("TabItemIndicator") as Brush
            ?? s_indicatorBrush;
    }

    private Brush ResolveCloseButtonHoverBrush()
    {
        return TryFindResource("SubtleFillColorSecondaryBrush") as Brush
            ?? TryFindResource("ControlBackgroundSecondary") as Brush
            ?? s_closeButtonHoverBrush;
    }

    private static void OnCloseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabViewItem tabViewItem)
        {
            tabViewItem._closeGlyphPen = null;
            tabViewItem.UpdateCloseButtonState();
        }
    }

    private static bool IsValidCloseButtonWidth(object? value)
    {
        return value is double width && double.IsFinite(width) && width >= 0;
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
