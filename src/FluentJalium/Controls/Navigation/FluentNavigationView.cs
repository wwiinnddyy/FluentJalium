using System.Collections.ObjectModel;
using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium native NavigationView - 100% custom implementation following WinUI 3,
/// ModernWPF, and WPF-UI Fluent Design Systems.
///
/// Features:
/// - Windows 11 island canvas framing (CornerRadius 8,0,0,0 + LayerFillColorDefault)
/// - Multi-mode sidebar presentation styles via <see cref="ItemStyle"/>:
///   * <see cref="FluentNavigationItemStyle.Tree"/> (WinUI 3 Canonical)
///   * <see cref="FluentNavigationItemStyle.Fluent"/> (WPF-UI / Windows Store / Settings)
/// - Robust, clean 2-column Layout: Column 0 = Animated Pane, Column 1 = Content Island
/// - Full support for Left, LeftCompact, LeftMinimal display modes
/// </summary>
public class FluentNavigationView : Control
{
    // WinUI / Fluent metrics
    internal const double DefaultOpenPaneLength = 320;
    internal const double DefaultCompactPaneLength = 48;
    private const double ToggleButtonRowHeight = 40;
    private const double ToggleButtonWidth = 40;
    private const double ToggleButtonHeight = 36;
    private const double ContentCornerRadius = 8;
    private static readonly TimeSpan s_paneAnimationDuration = TimeSpan.FromMilliseconds(250);

    private Grid? _rootGrid;
    private ColumnDefinition? _paneColumn;
    private ColumnDefinition? _contentColumn;
    private Border? _paneRoot;
    private Border? _contentRoot;
    private StackPanel? _menuItemsPanel;
    private StackPanel? _footerMenuItemsPanel;
    private Border? _toggleButton;
    private Border? _togglePill;
    private Grid? _toggleHolder;
    private Border? _paneHeaderBorder;
    private object? _paneHeader;
    private UIElement? _content;
    private bool _toggleMouseOver;
    private bool _togglePressed;

    public ObservableCollection<Control> MenuItems { get; } = new();
    public ObservableCollection<Control> FooterMenuItems { get; } = new();

    public event EventHandler<FluentNavigationViewSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<FluentNavigationViewBackRequestedEventArgs>? BackRequested;

    #region Dependency Properties

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(FluentNavigationView),
            new PropertyMetadata(true, OnPaneStatePropertyChanged));

    public static readonly DependencyProperty PaneDisplayModeProperty =
        DependencyProperty.Register(nameof(PaneDisplayMode), typeof(NavigationViewPaneDisplayMode), typeof(FluentNavigationView),
            new PropertyMetadata(NavigationViewPaneDisplayMode.Left, OnPaneStatePropertyChanged));

    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(FluentNavigationView),
            new PropertyMetadata(DefaultOpenPaneLength, OnPaneStatePropertyChanged));

    public static readonly DependencyProperty CompactPaneLengthProperty =
        DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(FluentNavigationView),
            new PropertyMetadata(DefaultCompactPaneLength, OnPaneStatePropertyChanged));

    public static readonly DependencyProperty PaneHeaderProperty =
        DependencyProperty.Register(nameof(PaneHeader), typeof(object), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnPaneHeaderChanged));

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnContentChanged));

    public static readonly DependencyProperty PaneBackgroundProperty =
        DependencyProperty.Register(nameof(PaneBackground), typeof(Brush), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnPaneChromeChanged));

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnPaneChromeChanged));

    public static readonly DependencyProperty ItemStyleProperty =
        DependencyProperty.Register(nameof(ItemStyle), typeof(FluentNavigationItemStyle), typeof(FluentNavigationView),
            new PropertyMetadata(FluentNavigationItemStyle.Tree, OnItemStyleChanged));

    #endregion

    #region Properties

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty)!;
        set => SetValue(IsPaneOpenProperty, value);
    }

    public NavigationViewPaneDisplayMode PaneDisplayMode
    {
        get => (NavigationViewPaneDisplayMode)GetValue(PaneDisplayModeProperty)!;
        set => SetValue(PaneDisplayModeProperty, value);
    }

    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty)!;
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty)!;
        set => SetValue(CompactPaneLengthProperty, value);
    }

    public object? PaneHeader
    {
        get => GetValue(PaneHeaderProperty);
        set => SetValue(PaneHeaderProperty, value);
    }

    public UIElement? Content
    {
        get => GetValue(ContentProperty) as UIElement;
        set => SetValue(ContentProperty, value);
    }

    public FluentNavigationItemStyle ItemStyle
    {
        get => (FluentNavigationItemStyle)GetValue(ItemStyleProperty)!;
        set => SetValue(ItemStyleProperty, value);
    }

    public Brush? PaneBackground
    {
        get => GetValue(PaneBackgroundProperty) as Brush;
        set => SetValue(PaneBackgroundProperty, value);
    }

    public Brush? ContentBackground
    {
        get => GetValue(ContentBackgroundProperty) as Brush;
        set => SetValue(ContentBackgroundProperty, value);
    }

    public bool IsPaneToggleButtonVisible
    {
        get => _toggleHolder?.Visibility == Visibility.Visible;
        set
        {
            if (_toggleHolder != null)
            {
                _toggleHolder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    public bool CanGoBack { get; set; }

    public bool IsCompactStrip { get; private set; }

    #endregion

    public FluentNavigationView()
    {
        BuildVisualTree();
        FluentThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += OnUnloaded;
    }

    #region Visual tree

    private void BuildVisualTree()
    {
        _rootGrid = new Grid
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };

        _paneColumn = new ColumnDefinition { Width = new GridLength(OpenPaneLength) };
        _contentColumn = new ColumnDefinition { Width = GridLength.Star };
        _rootGrid.ColumnDefinitions.Add(_paneColumn);
        _rootGrid.ColumnDefinitions.Add(_contentColumn);

        // Pane
        _paneRoot = CreatePaneRoot();
        Grid.SetColumn(_paneRoot, 0);
        _rootGrid.Children.Add(_paneRoot);

        // Content island card (Windows 11 canvas)
        _contentRoot = new Border();
        Grid.SetColumn(_contentRoot, 1);
        _rootGrid.Children.Add(_contentRoot);

        AddVisualChild(_rootGrid);

        UpdatePaneChrome();
        UpdatePaneState(animate: false);
    }

    private Border CreatePaneRoot()
    {
        var paneGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },  // Toggle button band
                new RowDefinition { Height = GridLength.Auto },  // Pane header
                new RowDefinition { Height = GridLength.Star },  // Menu items
                new RowDefinition { Height = GridLength.Auto }   // Footer items
            }
        };

        // Toggle button band
        _toggleHolder = new Grid
        {
            Height = ToggleButtonRowHeight,
            Margin = new Thickness(0, 4, 0, 4)
        };

        _togglePill = new Border
        {
            Margin = new Thickness(4, 2, 4, 2),
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Colors.Transparent),
            Child = FluentIconFactory.Segoe(SegoeFluentIcon.GlobalNavigationButton, 16,
                FluentNavigationViewItem.ResolveBrush("NavigationViewItemForeground"))
        };

        _toggleButton = new Border
        {
            Width = ToggleButtonWidth,
            Height = ToggleButtonHeight,
            Margin = new Thickness(4, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand,
            Child = _togglePill
        };
        _toggleButton.MouseEnter += (s, e) => { _toggleMouseOver = true; UpdateToggleButtonState(); };
        _toggleButton.MouseLeave += (s, e) => { _toggleMouseOver = false; _togglePressed = false; UpdateToggleButtonState(); };
        _toggleButton.MouseLeftButtonDown += (s, e) => { _togglePressed = true; UpdateToggleButtonState(); e.Handled = true; };
        _toggleButton.MouseLeftButtonUp += OnToggleButtonReleased;
        _toggleHolder.Children.Add(_toggleButton);
        Grid.SetRow(_toggleHolder, 0);
        paneGrid.Children.Add(_toggleHolder);

        // Menu items panel
        _menuItemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        var menuScrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _menuItemsPanel
        };
        Grid.SetRow(menuScrollViewer, 2);
        paneGrid.Children.Add(menuScrollViewer);

        // Footer items panel
        _footerMenuItemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 4)
        };
        Grid.SetRow(_footerMenuItemsPanel, 3);
        paneGrid.Children.Add(_footerMenuItemsPanel);

        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = paneGrid
        };
    }

    #endregion

    #region Pane state machine

    private void OnToggleButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        var invoke = _togglePressed && _toggleMouseOver;
        _togglePressed = false;
        UpdateToggleButtonState();
        if (invoke)
        {
            IsPaneOpen = !IsPaneOpen;
        }
        e.Handled = true;
    }

    private void UpdateToggleButtonState()
    {
        if (_togglePill == null) return;
        var key = _togglePressed
            ? "NavigationViewItemBackgroundPressed"
            : _toggleMouseOver
                ? "NavigationViewItemBackgroundHover"
                : "NavigationViewItemBackground";
        _togglePill.Background = FluentNavigationViewItem.ResolveBrush(key);
    }

    private void UpdatePaneState(bool animate = true)
    {
        if (_rootGrid == null || _paneColumn == null || _paneRoot == null || _contentRoot == null) return;

        double targetWidth;
        bool isHidden = false;

        switch (PaneDisplayMode)
        {
            case NavigationViewPaneDisplayMode.LeftCompact:
                targetWidth = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
                break;
            case NavigationViewPaneDisplayMode.LeftMinimal:
                if (!IsPaneOpen)
                {
                    targetWidth = 0;
                    isHidden = true;
                }
                else
                {
                    targetWidth = OpenPaneLength;
                }
                break;
            case NavigationViewPaneDisplayMode.Left:
            default:
                targetWidth = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
                break;
        }

        IsCompactStrip = !isHidden && targetWidth <= CompactPaneLength;

        // PROBE: corner radius disabled
        // _contentRoot.CornerRadius = (isHidden || PaneDisplayMode == NavigationViewPaneDisplayMode.LeftMinimal)
        //     ? new CornerRadius(0)
        //     : new CornerRadius(ContentCornerRadius, 0, 0, 0);

        _paneRoot.Visibility = isHidden ? Visibility.Collapsed : Visibility.Visible;
        _paneRoot.Background = PaneBackground ?? FluentNavigationViewItem.ResolveBrush("NavigationViewPaneBackground");

        SetPaneWidth(targetWidth, animate && !isHidden);

        // Compact-state propagation
        var compact = IsCompactStrip;
        foreach (var item in MenuItems)
        {
            if (item is FluentNavigationViewItem navItem) navItem.UpdateCompactState(compact);
            else if (item is NavigationViewItemHeader header) header.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }
        foreach (var item in FooterMenuItems)
        {
            if (item is FluentNavigationViewItem navItem) navItem.UpdateCompactState(compact);
        }
        if (_paneHeaderBorder != null)
        {
            _paneHeaderBorder.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void SetPaneWidth(double target, bool animate)
    {
        if (_paneColumn == null) return;

        // NOTE: pane-width animation is disabled — Jalium.UI's retained GPU layer compositor
        // currently leaves a stale composite of the content island at its pre-animation offset
        // (visible as a duplicated/ghosted page). Hard-set the column width until the
        // compositor damage tracking handles animated layout changes.
        _paneColumn.BeginAnimation(ColumnDefinition.WidthProperty, null);
        _paneColumn.Width = new GridLength(target);
    }

    #endregion

    #region Menu items

    public void UpdateMenuItems()
    {
        if (_menuItemsPanel == null) return;

        _menuItemsPanel.Children.Clear();

        if (_paneHeader is FrameworkElement paneHeaderElement && _paneRoot?.Child is Grid paneGrid)
        {
            if (_paneHeaderBorder != null && paneGrid.Children.Contains(_paneHeaderBorder))
            {
                _paneHeaderBorder.Child = null;
                paneGrid.Children.Remove(_paneHeaderBorder);
            }

            _paneHeaderBorder = new Border
            {
                Margin = new Thickness(16, 8, 16, 8),
            };

            if (paneHeaderElement.Parent is Visual oldParent)
            {
                if (oldParent is Panel panel)
                {
                    panel.Children.Remove(paneHeaderElement);
                }
                else if (oldParent is ContentControl cc && cc.Content == paneHeaderElement)
                {
                    cc.Content = null;
                }
            }

            _paneHeaderBorder.Child = paneHeaderElement;
            Grid.SetRow(_paneHeaderBorder, 1);
            paneGrid.Children.Add(_paneHeaderBorder);
        }
        else if (_paneHeaderBorder != null && _paneRoot?.Child is Grid grid)
        {
            if (grid.Children.Contains(_paneHeaderBorder))
            {
                _paneHeaderBorder.Child = null;
                grid.Children.Remove(_paneHeaderBorder);
                _paneHeaderBorder = null;
            }
        }

        foreach (var item in MenuItems)
        {
            if (item is FluentNavigationViewItem menuItem)
            {
                menuItem.ParentNavigationView = this;
                menuItem.ItemStyle = ItemStyle;
            }
            _menuItemsPanel.Children.Add(item);
        }

        if (_footerMenuItemsPanel != null)
        {
            _footerMenuItemsPanel.Children.Clear();
            foreach (var item in FooterMenuItems)
            {
                if (item is FluentNavigationViewItem footerItem)
                {
                    footerItem.ParentNavigationView = this;
                    footerItem.ItemStyle = ItemStyle;
                }
                _footerMenuItemsPanel.Children.Add(item);
            }
        }

        UpdatePaneState(animate: false);
    }

    internal void NotifyItemSelected(FluentNavigationViewItem item)
    {
        DeselectAllExcept(item, MenuItems);
        DeselectAllExcept(item, FooterMenuItems);

        SelectedItem = item;
        SelectionChanged?.Invoke(this, new FluentNavigationViewSelectionChangedEventArgs(item, null));
    }

    private static void DeselectAllExcept(FluentNavigationViewItem selected, IEnumerable<Control> items)
    {
        foreach (var menuItem in items.OfType<FluentNavigationViewItem>())
        {
            if (menuItem != selected)
            {
                menuItem.IsSelected = false;
            }
            DeselectAllExcept(selected, menuItem.MenuItems);
        }
    }

    #endregion

    #region Theme / chrome

    private void OnThemeChanged() => UpdatePaneChrome();

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        FluentThemeManager.ThemeChanged -= OnThemeChanged;
        Unloaded -= OnUnloaded;
    }

    private void UpdatePaneChrome()
    {
        if (_contentRoot != null)
        {
            // PROBE: transparent content like the pre-refactor baseline
            _contentRoot.Background = ContentBackground ??
                new SolidColorBrush(Colors.Transparent);
        }
        if (_togglePill?.Child is FluentIcon icon)
        {
            icon.Foreground = FluentNavigationViewItem.ResolveBrush("NavigationViewItemForeground");
        }
        UpdateToggleButtonState();
        UpdatePaneState(animate: false);
    }

    #endregion

    #region Property Changed Handlers

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            if (e.OldValue is FluentNavigationViewItem oldItem)
            {
                oldItem.IsSelected = false;
            }
            if (e.NewValue is FluentNavigationViewItem newItem)
            {
                newItem.IsSelected = true;
            }
        }
    }

    private static void OnPaneStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView.UpdatePaneState();
        }
    }

    private static void OnPaneHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView._paneHeader = e.NewValue;
            navView.UpdateMenuItems();
        }
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView && navView._contentRoot != null)
        {
            navView._content = e.NewValue as UIElement;
            navView._contentRoot.Child = navView._content;
        }
    }

    private static void OnPaneChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView.UpdatePaneChrome();
        }
    }

    private static void OnItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView && e.NewValue is FluentNavigationItemStyle style)
        {
            foreach (var item in navView.MenuItems.OfType<FluentNavigationViewItem>())
            {
                item.ItemStyle = style;
            }
            foreach (var item in navView.FooterMenuItems.OfType<FluentNavigationViewItem>())
            {
                item.ItemStyle = style;
            }
        }
    }

    #endregion

    #region Layout

    protected override Size MeasureOverride(Size availableSize)
    {
        _rootGrid?.Measure(availableSize);
        return _rootGrid?.DesiredSize ?? Size.Empty;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootGrid?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    protected override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _rootGrid : null;
    }

    protected override int VisualChildrenCount => _rootGrid != null ? 1 : 0;

    // This control builds its ENTIRE visual tree by hand in BuildVisualTree (_rootGrid)
    // and drives measure/arrange/render through the overrides above. It must never expand
    // a ControlTemplate: FluentThemeManager aliases the stock NavigationView Style — which
    // carries a full pane+content ControlTemplate — onto FWNavigationView, so without this
    // guard the base Control would build a second _templateRoot subtree and
    // Control.RenderTemplatedBackground would paint it every frame ON TOP of _rootGrid.
    // The result is two complete navigation UIs laid out at different pane/content offsets,
    // i.e. the "doubled / ghosted controls" artifact. Returning false keeps the hand-built
    // tree the single source of visuals; RenderTemplatedBackground is also neutralised so
    // no stray template root (from any future styling path) can ever be painted.
    protected override bool ApplyTemplateCore() => false;

    protected override void RenderTemplatedBackground(DrawingContext drawingContext)
    {
    }

    #endregion
}

#region Helper Animation

internal sealed class GridLengthAnimation : AnimationTimeline
{
    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register(nameof(From), typeof(GridLength?), typeof(GridLengthAnimation),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(nameof(To), typeof(GridLength?), typeof(GridLengthAnimation),
            new PropertyMetadata(null));

    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(nameof(EasingFunction), typeof(IEasingFunction), typeof(GridLengthAnimation),
            new PropertyMetadata(null));

    public GridLength? From
    {
        get => (GridLength?)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public GridLength? To
    {
        get => (GridLength?)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public IEasingFunction? EasingFunction
    {
        get => (IEasingFunction?)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public override Type TargetPropertyType => typeof(GridLength);

    protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

    public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
    {
        var progress = animationClock.CurrentProgress;

        if (EasingFunction is { } easing)
        {
            progress = easing.Ease(progress);
        }

        var from = From ?? (defaultOriginValue is GridLength g ? g : new GridLength(0));
        var to = To ?? (defaultDestinationValue is GridLength g2 ? g2 : new GridLength(0));

        var fromVal = from.Value;
        var toVal = to.Value;
        var current = fromVal + (toVal - fromVal) * progress;

        return new GridLength(Math.Max(0, current), from.GridUnitType);
    }
}

#endregion

#region Event Args

public class FluentNavigationViewSelectionChangedEventArgs : EventArgs
{
    public object? SelectedItem { get; }
    public object? PreviousItem { get; }

    public FluentNavigationViewSelectionChangedEventArgs(object? selectedItem, object? previousItem)
    {
        SelectedItem = selectedItem;
        PreviousItem = previousItem;
    }
}

public class FluentNavigationViewBackRequestedEventArgs : EventArgs
{
}

#endregion
