using System.Collections.ObjectModel;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium native NavigationView - 100% custom implementation following WinUI 3 Fluent Design System.
/// Does NOT depend on Jalium.UI's NavigationView to avoid limitations.
/// </summary>
public class FluentNavigationView : Control
{
    private Grid? _rootGrid;
    private Border? _paneRoot;
    private Border? _contentRoot;
    private StackPanel? _menuItemsPanel;
    private StackPanel? _footerMenuItemsPanel;
    private Button? _paneToggleButton;
    private Border? _paneHeaderBorder;
    private object? _paneHeader;
    private UIElement? _content;
    private bool _isPaneOpen = true;
    private NavigationViewPaneDisplayMode _paneDisplayMode = NavigationViewPaneDisplayMode.Left;
    private double _openPaneLength = 320;
    private double _compactPaneLength = 48;

    public ObservableCollection<FluentNavigationViewItem> MenuItems { get; } = new();
    public ObservableCollection<FluentNavigationViewItem> FooterMenuItems { get; } = new();

    public event EventHandler<FluentNavigationViewSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<FluentNavigationViewBackRequestedEventArgs>? BackRequested;

    #region Dependency Properties

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(FluentNavigationView),
            new PropertyMetadata(true, OnIsPaneOpenChanged));

    public static readonly DependencyProperty PaneDisplayModeProperty =
        DependencyProperty.Register(nameof(PaneDisplayMode), typeof(NavigationViewPaneDisplayMode), typeof(FluentNavigationView),
            new PropertyMetadata(NavigationViewPaneDisplayMode.Left, OnPaneDisplayModeChanged));

    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(FluentNavigationView),
            new PropertyMetadata(320.0, OnOpenPaneLengthChanged));

    public static readonly DependencyProperty CompactPaneLengthProperty =
        DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(FluentNavigationView),
            new PropertyMetadata(48.0, OnCompactPaneLengthChanged));

    public static readonly DependencyProperty PaneHeaderProperty =
        DependencyProperty.Register(nameof(PaneHeader), typeof(object), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnPaneHeaderChanged));

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(UIElement), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnContentChanged));

    public static readonly DependencyProperty PaneBackgroundProperty =
        DependencyProperty.Register(nameof(PaneBackground), typeof(Brush), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnPaneBackgroundChanged));

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(FluentNavigationView),
            new PropertyMetadata(null, OnContentBackgroundChanged));

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

    public bool IsPaneToggleButtonVisible { get; set; } = true;
    public bool CanGoBack { get; set; }

    #endregion

    public FluentNavigationView()
    {
        _isPaneOpen = true;
        _paneDisplayMode = NavigationViewPaneDisplayMode.Left;
        _openPaneLength = 320;
        _compactPaneLength = 48;

        BuildVisualTree();
    }

    private void BuildVisualTree()
    {
        _rootGrid = new Grid
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };

        // Two columns: Pane | Content
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_openPaneLength) });
        _rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        // Pane
        _paneRoot = CreatePaneRoot();
        Grid.SetColumn(_paneRoot, 0);
        _rootGrid.Children.Add(_paneRoot);

        // Content
        _contentRoot = CreateContentRoot();
        Grid.SetColumn(_contentRoot, 1);
        _rootGrid.Children.Add(_contentRoot);

        AddVisualChild(_rootGrid);
    }

    private Border CreatePaneRoot()
    {
        var paneGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },  // Toggle button
                new RowDefinition { Height = GridLength.Auto },  // Header
                new RowDefinition { Height = GridLength.Star },  // Menu items
                new RowDefinition { Height = GridLength.Auto }   // Footer items
            }
        };

        // Toggle button
        _paneToggleButton = new Button
        {
            Content = "☰",
            Width = 48,
            Height = 48,
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0),
            FontSize = 16,
            Visibility = IsPaneToggleButtonVisible ? Visibility.Visible : Visibility.Collapsed
        };
        _paneToggleButton.Click += OnPaneToggleButtonClick;
        Grid.SetRow(_paneToggleButton, 0);
        paneGrid.Children.Add(_paneToggleButton);

        // Menu items panel
        _menuItemsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 4, 0, 0)
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
            Background = PaneBackground ?? GetThemeBrush("FluentMaterialShellPaneBrush"),
            Child = paneGrid
        };
    }

    private Border CreateContentRoot()
    {
        return new Border
        {
            Background = ContentBackground ?? new SolidColorBrush(Colors.Transparent)
        };
    }

    public void UpdateMenuItems()
    {
        if (_menuItemsPanel == null) return;

        _menuItemsPanel.Children.Clear();

        // Update PaneHeader if set and is UIElement
        if (_paneHeader is UIElement paneHeaderElement && _paneRoot?.Child is Grid paneGrid)
        {
            // Remove old header border if exists
            if (_paneHeaderBorder != null && paneGrid.Children.Contains(_paneHeaderBorder))
            {
                // Clear the old child first to avoid "already has parent" error
                _paneHeaderBorder.Child = null;
                paneGrid.Children.Remove(_paneHeaderBorder);
            }

            // Create new header border
            _paneHeaderBorder = new Border
            {
                Margin = new Thickness(16, 12, 16, 8),
                Child = paneHeaderElement
            };
            Grid.SetRow(_paneHeaderBorder, 1);
            paneGrid.Children.Add(_paneHeaderBorder);
        }
        else if (_paneHeaderBorder != null && _paneRoot?.Child is Grid grid)
        {
            // Remove old header border if paneHeader is no longer valid
            if (grid.Children.Contains(_paneHeaderBorder))
            {
                _paneHeaderBorder.Child = null;
                grid.Children.Remove(_paneHeaderBorder);
                _paneHeaderBorder = null;
            }
        }

        foreach (var item in MenuItems)
        {
            item.ParentNavigationView = this;
            _menuItemsPanel.Children.Add(item);
        }

        if (_footerMenuItemsPanel != null)
        {
            _footerMenuItemsPanel.Children.Clear();
            foreach (var item in FooterMenuItems)
            {
                item.ParentNavigationView = this;
                _footerMenuItemsPanel.Children.Add(item);
            }
        }
    }

    internal void NotifyItemSelected(FluentNavigationViewItem item)
    {
        // Deselect all other items
        foreach (var menuItem in MenuItems)
        {
            if (menuItem != item)
            {
                menuItem.IsSelected = false;
            }
        }
        foreach (var footerItem in FooterMenuItems)
        {
            if (footerItem != item)
            {
                footerItem.IsSelected = false;
            }
        }

        SelectedItem = item;
        SelectionChanged?.Invoke(this, new FluentNavigationViewSelectionChangedEventArgs(item, null));
    }

    private void OnPaneToggleButtonClick(object? sender, RoutedEventArgs e)
    {
        IsPaneOpen = !IsPaneOpen;
    }

    private void UpdatePaneState()
    {
        if (_rootGrid == null || _paneRoot == null) return;

        double targetWidth = IsPaneOpen ? OpenPaneLength : CompactPaneLength;

        if (PaneDisplayMode == NavigationViewPaneDisplayMode.LeftCompact)
        {
            targetWidth = CompactPaneLength;
        }

        _rootGrid.ColumnDefinitions[0].Width = new GridLength(targetWidth);
    }

    #region Property Changed Handlers

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView && e.NewValue is FluentNavigationViewItem newItem)
        {
            newItem.IsSelected = true;
        }
    }

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView._isPaneOpen = (bool)e.NewValue!;
            navView.UpdatePaneState();
        }
    }

    private static void OnPaneDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView._paneDisplayMode = (NavigationViewPaneDisplayMode)e.NewValue!;
            navView.UpdatePaneState();
        }
    }

    private static void OnOpenPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView._openPaneLength = (double)e.NewValue!;
            navView.UpdatePaneState();
        }
    }

    private static void OnCompactPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView)
        {
            navView._compactPaneLength = (double)e.NewValue!;
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

    private static void OnPaneBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView && navView._paneRoot != null)
        {
            navView._paneRoot.Background = e.NewValue as Brush;
        }
    }

    private static void OnContentBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationView navView && navView._contentRoot != null)
        {
            navView._contentRoot.Background = e.NewValue as Brush;
        }
    }

    #endregion

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

    public override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _rootGrid : null;
    }

    public override int VisualChildrenCount => _rootGrid != null ? 1 : 0;

    private static Brush GetThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Color.FromRgb(240, 240, 240));
    }
}

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
